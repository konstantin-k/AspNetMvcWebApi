// Type: System.Web.Providers.DefaultRoleProvider
// Assembly: System.Web.Providers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// Assembly location: D:\Projects\WebApi\MvcOpenAccess\packages\Microsoft.AspNet.Providers.Core.1.1\lib\net40\System.Web.Providers.dll

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Globalization;
using System.Linq;
using System.Web.Providers.Entities;
using System.Web.Providers.Resources;
using System.Web.Security;

namespace System.Web.Providers
{
  public class DefaultRoleProvider : RoleProvider
  {
    public override string ApplicationName { get; set; }

    private ConnectionStringSettings ConnectionString { get; set; }

    public override void AddUsersToRoles(string[] usernames, string[] roleNames)
    {
      if (usernames == null)
        throw new ArgumentNullException("usernames");
      if (roleNames == null)
        throw new ArgumentNullException("roleNames");
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        if (QueryHelper.GetApplication(membershipEntities, this.ApplicationName) == null)
          ModelHelper.CreateApplication(membershipEntities, this.ApplicationName);
        List<User> list1 = new List<User>();
        foreach (string userName in usernames)
        {
          User user = QueryHelper.GetUser(membershipEntities, userName, this.ApplicationName);
          if (user == null)
            throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_this_user_not_found, new object[1]
            {
              (object) userName
            }));
          else
            list1.Add(user);
        }
        List<RoleEntity> list2 = new List<RoleEntity>();
        foreach (string roleName in roleNames)
        {
          RoleEntity role = QueryHelper.GetRole(membershipEntities, roleName, this.ApplicationName);
          if (role == null)
            throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_role_not_found, new object[1]
            {
              (object) roleName
            }));
          else
            list2.Add(role);
        }
        foreach (User user in list1)
        {
          foreach (RoleEntity roleEntity in list2)
          {
            if (QueryHelper.GetUserInRole(membershipEntities, this.ApplicationName, roleEntity.RoleName, user.UserName) != null)
              throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_this_user_already_in_role, new object[2]
              {
                (object) user.UserName,
                (object) roleEntity.RoleName
              }));
            else
              membershipEntities.UsersInRoles.AddObject(new UsersInRole()
              {
                UserId = user.UserId,
                RoleId = roleEntity.RoleId
              });
          }
        }
        membershipEntities.SaveChanges();
      }
    }

    public override void CreateRole(string roleName)
    {
      Exception exception = ValidationHelper.CheckParameter(ref roleName, true, true, true, 256, "roleName");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        Guid applicationId = (QueryHelper.GetApplication(membershipEntities, this.ApplicationName) ?? ModelHelper.CreateApplication(membershipEntities, this.ApplicationName)).ApplicationId;
        if (QueryHelper.GetRole(membershipEntities, roleName, this.ApplicationName) != null)
        {
          throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_role_already_exists, new object[1]
          {
            (object) roleName
          }));
        }
        else
        {
          membershipEntities.Roles.AddObject(new RoleEntity()
          {
            RoleId = Guid.NewGuid(),
            ApplicationId = applicationId,
            RoleName = roleName
          });
          membershipEntities.SaveChanges();
        }
      }
    }

    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
    {
      Exception exception = ValidationHelper.CheckParameter(ref roleName, true, true, true, 256, "roleName");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        RoleEntity role = QueryHelper.GetRole(membershipEntities, roleName, this.ApplicationName);
        if (role == null)
          return false;
        IQueryable<UsersInRole> userRolesInRole = QueryHelper.GetUserRolesInRole(membershipEntities, this.ApplicationName, roleName);
        if (throwOnPopulatedRole && Queryable.Count<UsersInRole>(userRolesInRole) > 0)
          throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Role_is_not_empty, new object[0]));
        foreach (UsersInRole entity in (IEnumerable<UsersInRole>) userRolesInRole)
          membershipEntities.UsersInRoles.DeleteObject(entity);
        membershipEntities.Roles.DeleteObject(role);
        membershipEntities.SaveChanges();
        return true;
      }
    }

    public override string[] FindUsersInRole(string roleName, string usernameToMatch)
    {
      Exception exception1 = ValidationHelper.CheckParameter(ref usernameToMatch, true, true, false, 256, "usernameToMatch");
      if (exception1 != null)
        throw exception1;
      Exception exception2 = ValidationHelper.CheckParameter(ref roleName, true, true, true, 256, "roleName");
      if (exception2 != null)
        throw exception2;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        if (QueryHelper.GetRole(membershipEntities, roleName, this.ApplicationName) != null)
          return QueryHelper.GetUserNamesInRole(membershipEntities, this.ApplicationName, roleName, usernameToMatch);
        throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_role_not_found, new object[1]
        {
          (object) roleName
        }));
      }
    }

    public override string[] GetAllRoles()
    {
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetAllRoleNames(membershipEntities, this.ApplicationName);
    }

    public override string[] GetRolesForUser(string username)
    {
      Exception exception = ValidationHelper.CheckParameter(ref username, true, false, true, 256, "username");
      if (exception != null)
        throw exception;
      if (username.Length < 1)
        return new string[0];
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetRolesNamesForUser(membershipEntities, this.ApplicationName, username);
    }

    public override string[] GetUsersInRole(string roleName)
    {
      Exception exception = ValidationHelper.CheckParameter(ref roleName, true, true, true, 256, "roleName");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetUserNamesInRole(membershipEntities, this.ApplicationName, roleName, "");
    }

    public override void Initialize(string name, NameValueCollection config)
    {
      if (config == null)
        throw new ArgumentNullException("config");
      if (string.IsNullOrEmpty(name))
        name = "DefaultRoleProvider";
      this.ConnectionString = ModelHelper.GetConnectionString(config["connectionStringName"]);
      config.Remove("connectionStringName");
      if (string.IsNullOrEmpty(config["description"]))
      {
        config.Remove("description");
        config.Add("description", string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.RoleProvider_description, new object[0]));
      }
      base.Initialize(name, config);
      if (!string.IsNullOrEmpty(config["applicationName"]))
        this.ApplicationName = config["applicationName"];
      else
        this.ApplicationName = ModelHelper.GetDefaultAppName();
      config.Remove("applicationName");
      if (config.Count <= 0)
        return;
      string key = config.GetKey(0);
      if (string.IsNullOrEmpty(key))
        return;
      throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_unrecognized_attribute, new object[1]
      {
        (object) key
      }));
    }

    public override bool IsUserInRole(string username, string roleName)
    {
      Exception exception1 = ValidationHelper.CheckParameter(ref username, true, false, true, 256, "username");
      if (exception1 != null)
        throw exception1;
      Exception exception2 = ValidationHelper.CheckParameter(ref roleName, true, true, true, 256, "roleName");
      if (exception2 != null)
        throw exception2;
      if (username.Length < 1)
        return false;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetUserInRole(membershipEntities, this.ApplicationName, roleName, username) != null;
    }

    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
    {
      if (usernames == null)
        throw new ArgumentNullException("usernames");
      if (roleNames == null)
        throw new ArgumentNullException("roleNames");
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        if (QueryHelper.GetApplication(membershipEntities, this.ApplicationName) == null)
          ModelHelper.CreateApplication(membershipEntities, this.ApplicationName);
        foreach (string userName in usernames)
        {
          if (QueryHelper.GetUser(membershipEntities, userName, this.ApplicationName) == null)
            throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_this_user_not_found, new object[1]
            {
              (object) userName
            }));
        }
        foreach (string roleName in roleNames)
        {
          if (QueryHelper.GetRole(membershipEntities, roleName, this.ApplicationName) == null)
            throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_role_not_found, new object[1]
            {
              (object) roleName
            }));
        }
        foreach (string userName in usernames)
        {
          foreach (string roleName in roleNames)
          {
            UsersInRole userInRole = QueryHelper.GetUserInRole(membershipEntities, this.ApplicationName, roleName, userName);
            if (userInRole == null)
              throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_this_user_already_not_in_role, new object[2]
              {
                (object) userName,
                (object) roleName
              }));
            else
              membershipEntities.UsersInRoles.DeleteObject(userInRole);
          }
        }
        membershipEntities.SaveChanges();
      }
    }

    public override bool RoleExists(string roleName)
    {
      Exception exception = ValidationHelper.CheckParameter(ref roleName, true, true, true, 256, "roleName");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetRole(membershipEntities, roleName, this.ApplicationName) != null;
    }
  }
}
