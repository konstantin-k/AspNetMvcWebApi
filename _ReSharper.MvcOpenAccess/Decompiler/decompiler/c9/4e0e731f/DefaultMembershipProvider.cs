// Type: System.Web.Providers.DefaultMembershipProvider
// Assembly: System.Web.Providers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// Assembly location: D:\Projects\WebApi\MvcOpenAccess\packages\Microsoft.AspNet.Providers.Core.1.1\lib\net40\System.Web.Providers.dll

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Providers.Entities;
using System.Web.Providers.Resources;
using System.Web.Security;

namespace System.Web.Providers
{
  public class DefaultMembershipProvider : MembershipProvider
  {
    internal static DateTime NullDate = new DateTime(1754, 1, 1);
    private MembershipPasswordCompatibilityMode _legacyPasswordCompatibilityMode = MembershipPasswordCompatibilityMode.Framework40;
    internal const int SALT_SIZE = 16;
    internal const int MaxPasswordSize = 128;
    internal const int MaxSimpleStringSize = 256;
    private Regex _PasswordStrengthRegEx;
    private string _HashAlgorithm;

    string ProviderName
    {
      private get
      {
        return this.Name;
      }
    }

    internal bool EnablePasswordResetInternal { get; set; }

    internal bool EnablePasswordRetrievalInternal { get; set; }

    internal int MaxInvalidPasswordAttemptsInternal { get; set; }

    internal int MinRequiredNonAlphanumericCharactersInternal { get; set; }

    internal int MinRequiredPasswordLengthInternal { get; set; }

    internal int PasswordAttemptWindowInternal { get; set; }

    internal MembershipPasswordFormat PasswordFormatInternal { get; set; }

    internal string PasswordStrengthRegularExpressionInternal { get; set; }

    internal bool RequiresQuestionAndAnswerInternal { get; set; }

    internal bool RequiresUniqueEmailInternal { get; set; }

    internal MembershipPasswordCompatibilityMode LegacyPasswordCompatibilityMode
    {
      get
      {
        return this._legacyPasswordCompatibilityMode;
      }
      set
      {
        this._legacyPasswordCompatibilityMode = value;
      }
    }

    public override string ApplicationName { get; set; }

    private ConnectionStringSettings ConnectionString { get; set; }

    public override bool EnablePasswordReset
    {
      get
      {
        return this.EnablePasswordResetInternal;
      }
    }

    public override bool EnablePasswordRetrieval
    {
      get
      {
        return this.EnablePasswordRetrievalInternal;
      }
    }

    public override int MaxInvalidPasswordAttempts
    {
      get
      {
        return this.MaxInvalidPasswordAttemptsInternal;
      }
    }

    public override int MinRequiredNonAlphanumericCharacters
    {
      get
      {
        return this.MinRequiredNonAlphanumericCharactersInternal;
      }
    }

    public override int MinRequiredPasswordLength
    {
      get
      {
        return this.MinRequiredPasswordLengthInternal;
      }
    }

    public override int PasswordAttemptWindow
    {
      get
      {
        return this.PasswordAttemptWindowInternal;
      }
    }

    public override MembershipPasswordFormat PasswordFormat
    {
      get
      {
        return this.PasswordFormatInternal;
      }
    }

    public override string PasswordStrengthRegularExpression
    {
      get
      {
        return this.PasswordStrengthRegularExpressionInternal;
      }
    }

    public override bool RequiresQuestionAndAnswer
    {
      get
      {
        return this.RequiresQuestionAndAnswerInternal;
      }
    }

    public override bool RequiresUniqueEmail
    {
      get
      {
        return this.RequiresUniqueEmailInternal;
      }
    }

    static DefaultMembershipProvider()
    {
    }

    public override void Initialize(string name, NameValueCollection config)
    {
      if (config == null)
        throw new ArgumentNullException("config");
      if (string.IsNullOrEmpty(name))
        name = "DefaultMembershipProvider";
      if (string.IsNullOrEmpty(config["description"]))
      {
        config.Remove("description");
        config.Add("description", ProviderResources.MembershipProvider_description);
      }
      base.Initialize(name, config);
      if (!string.IsNullOrEmpty(config["applicationName"]))
        this.ApplicationName = config["applicationName"];
      else
        this.ApplicationName = ModelHelper.GetDefaultAppName();
      this.ConnectionString = ModelHelper.GetConnectionString(config["connectionStringName"]);
      config.Remove("connectionStringName");
      this.EnablePasswordResetInternal = config["enablePasswordReset"] == null || Convert.ToBoolean(config["enablePasswordReset"], (IFormatProvider) CultureInfo.InvariantCulture);
      this.EnablePasswordRetrievalInternal = config["enablePasswordRetrieval"] != null && Convert.ToBoolean(config["enablePasswordRetrieval"], (IFormatProvider) CultureInfo.InvariantCulture);
      this.MaxInvalidPasswordAttemptsInternal = config["maxInvalidPasswordAttempts"] == null ? 5 : Convert.ToInt32(config["maxInvalidPasswordAttempts"], (IFormatProvider) CultureInfo.InvariantCulture);
      this.MinRequiredNonAlphanumericCharactersInternal = config["minRequiredNonalphanumericCharacters"] == null ? 1 : Convert.ToInt32(config["minRequiredNonalphanumericCharacters"], (IFormatProvider) CultureInfo.InvariantCulture);
      this.MinRequiredPasswordLengthInternal = config["minRequiredPasswordLength"] == null ? 7 : Convert.ToInt32(config["minRequiredPasswordLength"], (IFormatProvider) CultureInfo.InvariantCulture);
      this.PasswordAttemptWindowInternal = config["passwordAttemptWindow"] == null ? 10 : Convert.ToInt32(config["passwordAttemptWindow"], (IFormatProvider) CultureInfo.InvariantCulture);
      this.PasswordFormatInternal = config["passwordFormat"] == null ? MembershipPasswordFormat.Hashed : (MembershipPasswordFormat) Enum.Parse(typeof (MembershipPasswordFormat), config["passwordFormat"]);
      if (config["passwordStrengthRegularExpression"] != null)
      {
        this.PasswordStrengthRegularExpressionInternal = config["passwordStrengthRegularExpression"];
        try
        {
          this._PasswordStrengthRegEx = new Regex(this.PasswordStrengthRegularExpressionInternal);
        }
        catch (ArgumentException ex)
        {
          throw new ProviderException(ex.Message, (Exception) ex);
        }
      }
      else
        this.PasswordStrengthRegularExpressionInternal = string.Empty;
      this.RequiresQuestionAndAnswerInternal = config["requiresQuestionAndAnswer"] == null || Convert.ToBoolean(config["requiresQuestionAndAnswer"], (IFormatProvider) CultureInfo.InvariantCulture);
      this.RequiresUniqueEmailInternal = config["requiresUniqueEmail"] == null || Convert.ToBoolean(config["requiresUniqueEmail"], (IFormatProvider) CultureInfo.InvariantCulture);
      if (this.PasswordFormat == MembershipPasswordFormat.Hashed && this.EnablePasswordRetrieval)
        throw new ProviderException(ProviderResources.Provider_can_not_retrieve_hashed_password);
      string str = config["passwordCompatMode"];
      if (!string.IsNullOrEmpty(str))
        this.LegacyPasswordCompatibilityMode = (MembershipPasswordCompatibilityMode) Enum.Parse(typeof (MembershipPasswordCompatibilityMode), str);
      config.Remove("applicationName");
      config.Remove("enablePasswordReset");
      config.Remove("enablePasswordRetrieval");
      config.Remove("maxInvalidPasswordAttempts");
      config.Remove("minRequiredNonalphanumericCharacters");
      config.Remove("minRequiredPasswordLength");
      config.Remove("passwordAttemptWindow");
      config.Remove("passwordFormat");
      config.Remove("passwordStrengthRegularExpression");
      config.Remove("requiresQuestionAndAnswer");
      config.Remove("requiresUniqueEmail");
      config.Remove("passwordCompatMode");
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

    public override bool ChangePassword(string username, string oldPassword, string newPassword)
    {
      Exception exception1 = ValidationHelper.CheckParameter(ref username, true, true, true, 256, "username");
      if (exception1 != null)
        throw exception1;
      Exception exception2 = ValidationHelper.CheckParameter(ref oldPassword, true, true, false, 128, "oldPassword");
      if (exception2 != null)
        throw exception2;
      Exception exception3 = ValidationHelper.CheckParameter(ref newPassword, true, true, false, 128, "newPassword");
      if (exception3 != null)
        throw exception3;
      string salt;
      int passwordFormat;
      if (!this.CheckPassword(username, oldPassword, false, false, out salt, out passwordFormat))
        return false;
      if (newPassword.Length < this.MinRequiredPasswordLength)
      {
        throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Password_too_short, new object[2]
        {
          (object) "newPassword",
          (object) this.MinRequiredPasswordLength.ToString((IFormatProvider) CultureInfo.InvariantCulture)
        }), "newPassword");
      }
      else
      {
        if (this.MinRequiredNonAlphanumericCharacters > 0)
        {
          int num = 0;
          for (int index = 0; index < newPassword.Length; ++index)
          {
            if (!char.IsLetterOrDigit(newPassword[index]))
              ++num;
          }
          if (num < this.MinRequiredNonAlphanumericCharacters)
            throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Password_need_more_non_alpha_numeric_chars, new object[2]
            {
              (object) "newPassword",
              (object) this.MinRequiredNonAlphanumericCharacters.ToString((IFormatProvider) CultureInfo.InvariantCulture)
            }), "newPassword");
        }
        if (this._PasswordStrengthRegEx != null && !this._PasswordStrengthRegEx.IsMatch(newPassword))
        {
          throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Password_does_not_match_regular_expression, new object[1]
          {
            (object) "newPassword"
          }), "newPassword");
        }
        else
        {
          ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, true);
          this.OnValidatingPassword(e);
          if (e.Cancel)
          {
            if (e.FailureInformation != null)
              throw e.FailureInformation;
            else
              throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Membership_Custom_Password_Validation_Failure, new object[0]), "newPassword");
          }
          else
          {
            string str = this.EncodePassword(newPassword, passwordFormat, salt);
            using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
            {
              MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, username);
              if (membership == null)
                return false;
              membership.Password = str;
              membership.PasswordSalt = salt;
              membership.LastPasswordChangedDate = DateTime.UtcNow;
              membershipEntities.SaveChanges();
              return true;
            }
          }
        }
      }
    }

    public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
    {
      Exception exception1 = ValidationHelper.CheckParameter(ref username, true, true, true, 256, "username");
      if (exception1 != null)
        throw exception1;
      Exception exception2 = ValidationHelper.CheckParameter(ref password, true, true, false, 128, "password");
      if (exception2 != null)
        throw exception2;
      string salt;
      int passwordFormat;
      if (!this.CheckPassword(username, password, false, false, out salt, out passwordFormat))
        return false;
      Exception exception3 = ValidationHelper.CheckParameter(ref newPasswordQuestion, this.RequiresQuestionAndAnswer, this.RequiresQuestionAndAnswer, false, 256, "newPasswordQuestion");
      if (exception3 != null)
        throw exception3;
      Exception exception4 = ValidationHelper.CheckParameter(ref newPasswordAnswer, this.RequiresQuestionAndAnswer, this.RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");
      if (exception4 != null)
        throw exception4;
      string str = string.IsNullOrEmpty(newPasswordAnswer) ? newPasswordAnswer : this.EncodePassword(newPasswordAnswer, passwordFormat, salt);
      Exception exception5 = ValidationHelper.CheckParameter(ref str, this.RequiresQuestionAndAnswer, this.RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");
      if (exception5 != null)
        throw exception5;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, username);
        if (membership == null)
          return false;
        membership.PasswordQuestion = newPasswordQuestion;
        membership.PasswordAnswer = str;
        membershipEntities.SaveChanges();
        return true;
      }
    }

    public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
    {
      string salt = DefaultMembershipProvider.GenerateSalt();
      if (!ValidationHelper.ValidateParameter(ref password, true, true, false, 128))
      {
        status = MembershipCreateStatus.InvalidPassword;
        return (MembershipUser) null;
      }
      else
      {
        string password1 = this.EncodePassword(password, (int) this.PasswordFormat, salt);
        if (passwordAnswer != null)
          passwordAnswer = passwordAnswer.Trim();
        string passwordAnswer1 = string.IsNullOrEmpty(passwordAnswer) ? passwordAnswer : this.EncodePassword(passwordAnswer, (int) this.PasswordFormat, salt);
        if (!ValidationHelper.ValidateParameter(ref passwordAnswer1, this.RequiresQuestionAndAnswer, true, false, 128))
        {
          status = MembershipCreateStatus.InvalidAnswer;
          return (MembershipUser) null;
        }
        else if (!ValidationHelper.ValidateParameter(ref passwordQuestion, this.RequiresQuestionAndAnswer, true, false, 256))
        {
          status = MembershipCreateStatus.InvalidQuestion;
          return (MembershipUser) null;
        }
        else if (!ValidationHelper.ValidateParameter(ref username, true, true, true, 256))
        {
          status = MembershipCreateStatus.InvalidUserName;
          return (MembershipUser) null;
        }
        else if (!ValidationHelper.ValidateParameter(ref email, this.RequiresUniqueEmail, this.RequiresUniqueEmail, false, 256))
        {
          status = MembershipCreateStatus.InvalidEmail;
          return (MembershipUser) null;
        }
        else if (providerUserKey != null && !(providerUserKey is Guid))
        {
          status = MembershipCreateStatus.InvalidProviderUserKey;
          return (MembershipUser) null;
        }
        else if (password == null || password.Length < this.MinRequiredPasswordLength)
        {
          status = MembershipCreateStatus.InvalidPassword;
          return (MembershipUser) null;
        }
        else
        {
          if (this.MinRequiredNonAlphanumericCharacters > 0)
          {
            int num = 0;
            for (int index = 0; index < password.Length; ++index)
            {
              if (!char.IsLetterOrDigit(password[index]))
                ++num;
            }
            if (num < this.MinRequiredNonAlphanumericCharacters)
            {
              status = MembershipCreateStatus.InvalidPassword;
              return (MembershipUser) null;
            }
          }
          if (this._PasswordStrengthRegEx != null && !this._PasswordStrengthRegEx.IsMatch(password))
          {
            status = MembershipCreateStatus.InvalidPassword;
            return (MembershipUser) null;
          }
          else
          {
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
            this.OnValidatingPassword(e);
            if (e.Cancel)
            {
              status = MembershipCreateStatus.InvalidPassword;
              return (MembershipUser) null;
            }
            else
            {
              DateTime createDate;
              int num = this.Membership_CreateUser(this.ApplicationName, username, password1, salt, email, passwordQuestion, passwordAnswer1, isApproved, out createDate, this.RequiresUniqueEmail, (int) this.PasswordFormat, ref providerUserKey);
              if (num < 0 || num > 11)
                num = 11;
              status = (MembershipCreateStatus) num;
              if (status != MembershipCreateStatus.Success)
                return (MembershipUser) null;
              else
                return new MembershipUser(this.ProviderName, username, providerUserKey, email, passwordQuestion, (string) null, isApproved, false, createDate, createDate, createDate, createDate, DefaultMembershipProvider.NullDate);
            }
          }
        }
      }
    }

    public override bool DeleteUser(string username, bool deleteAllRelatedData)
    {
      Exception exception = ValidationHelper.CheckParameter(ref username, true, true, true, 256, "username");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, username);
        if (membership == null)
          return false;
        membershipEntities.Memberships.DeleteObject(membership);
        if (deleteAllRelatedData)
        {
          foreach (UsersInRole entity in (IEnumerable<UsersInRole>) QueryHelper.GetRolesForUser(membershipEntities, this.ApplicationName, username))
            membershipEntities.UsersInRoles.DeleteObject(entity);
          ProfileEntity profile = QueryHelper.GetProfile(membershipEntities, this.ApplicationName, username);
          if (profile != null)
            membershipEntities.Profiles.DeleteObject(profile);
          User user = QueryHelper.GetUser(membershipEntities, membership.UserId, this.ApplicationName);
          if (user != null)
            membershipEntities.Users.DeleteObject(user);
        }
        membershipEntities.SaveChanges();
        return true;
      }
    }

    public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
      Exception exception = ValidationHelper.CheckParameter(ref emailToMatch, this.RequiresUniqueEmail, this.RequiresUniqueEmail, false, 256, "emailToMatch");
      if (exception != null)
        throw exception;
      MembershipUserCollection membershipUserCollection = new MembershipUserCollection();
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        totalRecords = Queryable.Count<DbDataRecord>(QueryHelper.GetAllMembershipUsersLikeEmail(membershipEntities, this.ApplicationName, emailToMatch, -1, -1));
        foreach (DbDataRecord record in (IEnumerable<DbDataRecord>) QueryHelper.GetAllMembershipUsersLikeEmail(membershipEntities, this.ApplicationName, emailToMatch, pageIndex, pageSize))
          membershipUserCollection.Add(QueryHelper.CreateMembershipUserFromDbRecord(membershipEntities, record, this.ProviderName, this.ApplicationName, false));
      }
      return membershipUserCollection;
    }

    public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
      Exception exception = ValidationHelper.CheckParameter(ref usernameToMatch, true, true, true, 256, "usernameToMatch");
      if (exception != null)
        throw exception;
      MembershipUserCollection membershipUserCollection = new MembershipUserCollection();
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        totalRecords = Queryable.Count<DbDataRecord>(QueryHelper.GetAllMembershipUsersLikeUserName(membershipEntities, this.ApplicationName, usernameToMatch, -1, -1));
        foreach (DbDataRecord record in (IEnumerable<DbDataRecord>) QueryHelper.GetAllMembershipUsersLikeUserName(membershipEntities, this.ApplicationName, usernameToMatch, pageIndex, pageSize))
          membershipUserCollection.Add(QueryHelper.CreateMembershipUserFromDbRecord(membershipEntities, record, this.ProviderName, this.ApplicationName, false));
      }
      return membershipUserCollection;
    }

    public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
    {
      MembershipUserCollection membershipUserCollection = new MembershipUserCollection();
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        totalRecords = Queryable.Count<DbDataRecord>(QueryHelper.GetAllMembershipUsers(membershipEntities, this.ApplicationName, -1, -1));
        foreach (DbDataRecord record in (IEnumerable<DbDataRecord>) QueryHelper.GetAllMembershipUsers(membershipEntities, this.ApplicationName, pageIndex, pageSize))
          membershipUserCollection.Add(QueryHelper.CreateMembershipUserFromDbRecord(membershipEntities, record, this.ProviderName, this.ApplicationName, false));
      }
      return membershipUserCollection;
    }

    public override int GetNumberOfUsersOnline()
    {
      DateTime dateactive = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes((double) Membership.UserIsOnlineTimeWindow));
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetNumberOfOnlineUsers(membershipEntities, this.ApplicationName, dateactive);
    }

    public override string GetPassword(string username, string answer)
    {
      if (!this.EnablePasswordRetrieval)
        throw new NotSupportedException(ProviderResources.Membership_PasswordRetrieval_not_supported);
      Exception exception1 = ValidationHelper.CheckParameter(ref username, true, true, true, 256, "username");
      if (exception1 != null)
        throw exception1;
      string strB = answer == null ? answer : this.GetEncodedPasswordAnswer(username, answer);
      Exception exception2 = ValidationHelper.CheckParameter(ref strB, this.RequiresQuestionAndAnswer, this.RequiresQuestionAndAnswer, false, 128, "passwordAnswer");
      if (exception2 != null)
        throw exception2;
      int status = 0;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, username);
        if (membership == null)
          status = 1;
        else if (membership.IsLockedOut)
        {
          status = 99;
        }
        else
        {
          DateTime utcNow = DateTime.UtcNow;
          if (this.RequiresQuestionAndAnswer && membership.PasswordAnswer != null)
          {
            if (string.Compare(membership.PasswordAnswer, strB, StringComparison.OrdinalIgnoreCase) != 0)
            {
              membership.FailedPasswordAnswerAttemptWindowsStart = utcNow;
              if (utcNow > membership.FailedPasswordAnswerAttemptWindowsStart.AddMinutes((double) this.PasswordAttemptWindow))
                membership.FailedPasswordAnswerAttemptCount = 1;
              else
                ++membership.FailedPasswordAnswerAttemptCount;
              if (membership.FailedPasswordAnswerAttemptCount >= this.MaxInvalidPasswordAttempts)
              {
                membership.IsLockedOut = true;
                membership.LastLockoutDate = utcNow;
              }
              status = 3;
            }
            else if (membership.FailedPasswordAnswerAttemptCount > 0)
            {
              membership.FailedPasswordAnswerAttemptCount = 0;
              membership.FailedPasswordAnswerAttemptWindowsStart = DefaultMembershipProvider.NullDate;
            }
            membershipEntities.SaveChanges();
          }
        }
        if (status == 0)
        {
          if (membership.Password != null)
            return this.UnEncodePassword(membership.Password, membership.PasswordFormat);
        }
      }
      DefaultMembershipProvider.ValidateStatus(status);
      return (string) null;
    }

    public override MembershipUser GetUser(string username, bool userIsOnline)
    {
      Exception exception = ValidationHelper.CheckParameter(ref username, true, false, true, 256, "username");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetMembershipUser(membershipEntities, username, this.ApplicationName, userIsOnline, this.ProviderName);
    }

    public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
    {
      if (providerUserKey == null)
        throw new ArgumentNullException("providerUserKey");
      if (!(providerUserKey is Guid))
        throw new ArgumentException(ProviderResources.Membership_InvalidProviderUserKey, "providerUserKey");
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetMembershipUser(membershipEntities, (Guid) providerUserKey, this.ApplicationName, userIsOnline, this.ProviderName);
    }

    public override string GetUserNameByEmail(string email)
    {
      Exception exception = ValidationHelper.CheckParameter(ref email, this.RequiresUniqueEmail, this.RequiresUniqueEmail, false, 256, "email");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        return QueryHelper.GetUserNameFromEmail(membershipEntities, email, this.ApplicationName);
    }

    private static void ValidateStatus(int status)
    {
      if (status == 0)
        return;
      string exceptionText = DefaultMembershipProvider.GetExceptionText(status);
      if (DefaultMembershipProvider.IsStatusDueToBadPassword(status))
        throw new MembershipPasswordException(exceptionText);
      else
        throw new ProviderException(exceptionText);
    }

    private static string GetExceptionText(int status)
    {
      switch (status)
      {
        case 0:
          return string.Empty;
        case 1:
          return ProviderResources.Membership_UserNotFound;
        case 2:
          return ProviderResources.Membership_WrongPassword;
        case 3:
          return ProviderResources.Membership_WrongAnswer;
        case 4:
          return ProviderResources.Membership_InvalidPassword;
        case 5:
          return ProviderResources.Membership_InvalidQuestion;
        case 6:
          return ProviderResources.Membership_InvalidAnswer;
        case 7:
          return ProviderResources.Membership_InvalidEmail;
        case 99:
          return ProviderResources.Membership_AccountLockOut;
        default:
          return ProviderResources.Provider_Error;
      }
    }

    private static bool IsStatusDueToBadPassword(int status)
    {
      if (status < 2 || status > 6)
        return status == 99;
      else
        return true;
    }

    public override string ResetPassword(string username, string answer)
    {
      if (!this.EnablePasswordReset)
        throw new NotSupportedException(ProviderResources.Not_configured_to_support_password_resets);
      Exception exception1 = ValidationHelper.CheckParameter(ref username, true, true, true, 256, "username");
      if (exception1 != null)
        throw exception1;
      int status1;
      string password;
      int format;
      string salt;
      int failedPasswordAttemptCount;
      int failedPasswordAnswerAttemptCount;
      bool isApproved;
      DateTime lastLoginDate;
      DateTime lastActivityDate;
      this.GetPasswordWithFormat(username, false, out status1, out password, out format, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
      DefaultMembershipProvider.ValidateStatus(status1);
      if (answer != null)
        answer = answer.Trim();
      string strB = string.IsNullOrEmpty(answer) ? answer : this.EncodePassword(answer, format, salt);
      Exception exception2 = ValidationHelper.CheckParameter(ref strB, this.RequiresQuestionAndAnswer, this.RequiresQuestionAndAnswer, false, 128, "answer");
      if (exception2 != null)
        throw exception2;
      string str = Membership.GeneratePassword(this.MinRequiredPasswordLength < 14 ? 14 : this.MinRequiredPasswordLength, this.MinRequiredNonAlphanumericCharacters);
      ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, str, false);
      this.OnValidatingPassword(e);
      if (e.Cancel)
      {
        if (e.FailureInformation != null)
          throw e.FailureInformation;
        else
          throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Membership_Custom_Password_Validation_Failure, new object[0]));
      }
      else
      {
        using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
        {
          int status2 = 0;
          DateTime utcNow = DateTime.UtcNow;
          MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, username);
          if (membership == null)
            status2 = 1;
          else if (membership.IsLockedOut)
          {
            status2 = 99;
          }
          else
          {
            if (answer == null || string.Compare(membership.PasswordAnswer, strB, StringComparison.OrdinalIgnoreCase) == 0)
            {
              membership.Password = this.EncodePassword(str, format, salt);
              membership.LastPasswordChangedDate = DateTime.UtcNow;
              membership.PasswordFormat = format;
              membership.PasswordSalt = salt;
              if (membership.FailedPasswordAnswerAttemptCount > 0)
              {
                membership.FailedPasswordAnswerAttemptCount = 0;
                membership.FailedPasswordAnswerAttemptWindowsStart = DefaultMembershipProvider.NullDate;
              }
            }
            else
            {
              if (utcNow > membership.FailedPasswordAnswerAttemptWindowsStart.AddMinutes((double) this.PasswordAttemptWindow))
                membership.FailedPasswordAnswerAttemptCount = 1;
              else
                ++membership.FailedPasswordAnswerAttemptCount;
              membership.FailedPasswordAnswerAttemptWindowsStart = utcNow;
              if (membership.FailedPasswordAnswerAttemptCount >= this.MaxInvalidPasswordAttempts)
              {
                membership.IsLockedOut = true;
                membership.LastLockoutDate = utcNow;
              }
              status2 = 3;
            }
            membershipEntities.SaveChanges();
          }
          DefaultMembershipProvider.ValidateStatus(status2);
        }
        return str;
      }
    }

    public override bool UnlockUser(string userName)
    {
      Exception exception = ValidationHelper.CheckParameter(ref userName, true, true, true, 256, "username");
      if (exception != null)
        throw exception;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, userName);
        if (membership == null)
          return false;
        membership.IsLockedOut = false;
        membership.FailedPasswordAnswerAttemptCount = 0;
        membership.FailedPasswordAnswerAttemptWindowsStart = DefaultMembershipProvider.NullDate;
        membership.FailedPasswordAttemptCount = 0;
        membership.FailedPasswordAttemptWindowStart = DefaultMembershipProvider.NullDate;
        membership.LastLockoutDate = DefaultMembershipProvider.NullDate;
        membershipEntities.SaveChanges();
        return true;
      }
    }

    public override void UpdateUser(MembershipUser user)
    {
      if (user == null)
        throw new ArgumentNullException("user");
      string userName = user.UserName;
      Exception exception1 = ValidationHelper.CheckParameter(ref userName, true, true, true, 256, "user.UserName");
      if (exception1 != null)
        throw exception1;
      string email = user.Email;
      Exception exception2 = ValidationHelper.CheckParameter(ref email, this.RequiresUniqueEmail, this.RequiresUniqueEmail, false, 256, "user.Email");
      if (exception2 != null)
        throw exception2;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        int status = 0;
        Guid? nullable = (Guid?) user.ProviderUserKey;
        if (!nullable.HasValue)
          status = 1;
        if (status == 0 && this.RequiresUniqueEmail && QueryHelper.DuplicateEmailExists(membershipEntities, this.ApplicationName, nullable.Value, user.Email))
          status = 7;
        if (status == 0)
        {
          QueryHelper.GetUser(membershipEntities, nullable.Value, this.ApplicationName).LastActivityDate = DateTime.UtcNow;
          MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, nullable.Value);
          membership.Email = user.Email;
          membership.Comment = user.Comment;
          membership.IsApproved = user.IsApproved;
          membership.LastLoginDate = user.LastLoginDate;
          membershipEntities.SaveChanges();
        }
        else
          DefaultMembershipProvider.ValidateStatus(status);
      }
    }

    public override bool ValidateUser(string username, string password)
    {
      return ValidationHelper.ValidateParameter(ref password, true, true, false, 128) && ValidationHelper.ValidateParameter(ref username, true, true, true, 256) && this.CheckPassword(username, password, true, true);
    }

    private static string GenerateSalt()
    {
      using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
      {
        byte[] numArray = new byte[16];
        cryptoServiceProvider.GetBytes(numArray);
        return Convert.ToBase64String(numArray);
      }
    }

    internal HashAlgorithm GetHashAlgorithm()
    {
      if (this._HashAlgorithm != null)
        return HashAlgorithm.Create(this._HashAlgorithm);
      string hashName = Membership.HashAlgorithmType;
      if (this.LegacyPasswordCompatibilityMode == MembershipPasswordCompatibilityMode.Framework20 && hashName != "MD5")
        hashName = "SHA1";
      HashAlgorithm hashAlgorithm = HashAlgorithm.Create(hashName);
      if (hashAlgorithm == null)
      {
        throw new ConfigurationErrorsException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Invalid_hash_algorithm, new object[1]
        {
          (object) this._HashAlgorithm
        }));
      }
      else
      {
        this._HashAlgorithm = hashName;
        return hashAlgorithm;
      }
    }

    private string EncodePassword(string pass, int passwordFormat, string salt)
    {
      if (passwordFormat == 0)
        return pass;
      byte[] bytes = Encoding.Unicode.GetBytes(pass);
      byte[] numArray1 = Convert.FromBase64String(salt);
      byte[] inArray;
      if (passwordFormat == 1)
      {
        HashAlgorithm hashAlgorithm = this.GetHashAlgorithm();
        KeyedHashAlgorithm keyedHashAlgorithm = hashAlgorithm as KeyedHashAlgorithm;
        if (keyedHashAlgorithm != null)
        {
          if (keyedHashAlgorithm.Key.Length == numArray1.Length)
            keyedHashAlgorithm.Key = numArray1;
          else if (keyedHashAlgorithm.Key.Length < numArray1.Length)
          {
            byte[] numArray2 = new byte[keyedHashAlgorithm.Key.Length];
            Buffer.BlockCopy((Array) numArray1, 0, (Array) numArray2, 0, numArray2.Length);
            keyedHashAlgorithm.Key = numArray2;
          }
          else
          {
            byte[] numArray2 = new byte[keyedHashAlgorithm.Key.Length];
            int dstOffset = 0;
            while (dstOffset < numArray2.Length)
            {
              int count = Math.Min(numArray1.Length, numArray2.Length - dstOffset);
              Buffer.BlockCopy((Array) numArray1, 0, (Array) numArray2, dstOffset, count);
              dstOffset += count;
            }
            keyedHashAlgorithm.Key = numArray2;
          }
          inArray = keyedHashAlgorithm.ComputeHash(bytes);
        }
        else
        {
          byte[] buffer = new byte[numArray1.Length + bytes.Length];
          Buffer.BlockCopy((Array) numArray1, 0, (Array) buffer, 0, numArray1.Length);
          Buffer.BlockCopy((Array) bytes, 0, (Array) buffer, numArray1.Length, bytes.Length);
          inArray = hashAlgorithm.ComputeHash(buffer);
        }
      }
      else
      {
        byte[] password = new byte[numArray1.Length + bytes.Length];
        Buffer.BlockCopy((Array) numArray1, 0, (Array) password, 0, numArray1.Length);
        Buffer.BlockCopy((Array) bytes, 0, (Array) password, numArray1.Length, bytes.Length);
        inArray = this.EncryptPassword(password, this.LegacyPasswordCompatibilityMode);
      }
      return Convert.ToBase64String(inArray);
    }

    private string UnEncodePassword(string pass, int passwordFormat)
    {
      switch (passwordFormat)
      {
        case 0:
          return pass;
        case 1:
          throw new ProviderException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ProviderResources.Provider_can_not_decode_hashed_password, new object[0]));
        default:
          byte[] bytes = this.DecryptPassword(Convert.FromBase64String(pass));
          if (bytes == null)
            return (string) null;
          else
            return Encoding.Unicode.GetString(bytes, 16, bytes.Length - 16);
      }
    }

    private bool CheckPassword(string userName, string password, bool updateLastActivityDate, bool failIfNotApproved)
    {
      string salt;
      int passwordFormat;
      return this.CheckPassword(userName, password, updateLastActivityDate, failIfNotApproved, out salt, out passwordFormat);
    }

    private bool CheckPassword(string userName, string password, bool updateLastActivityDate, bool failIfNotApproved, out string salt, out int passwordFormat)
    {
      int status;
      string password1;
      int failedPasswordAttemptCount;
      int failedPasswordAnswerAttemptCount;
      bool isApproved;
      DateTime lastLoginDate;
      DateTime lastActivityDate;
      this.GetPasswordWithFormat(userName, updateLastActivityDate, out status, out password1, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
      if (status != 0 || !isApproved && failIfNotApproved)
        return false;
      bool flag = string.Compare(this.EncodePassword(password, passwordFormat, salt), password1, StringComparison.Ordinal) == 0;
      DateTime utcNow = DateTime.UtcNow;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, userName);
        if (membership != null && !membership.IsLockedOut)
        {
          if (flag)
          {
            if (membership.FailedPasswordAttemptCount > 0 || membership.FailedPasswordAnswerAttemptCount > 0)
            {
              membership.FailedPasswordAnswerAttemptCount = 0;
              membership.FailedPasswordAnswerAttemptWindowsStart = DefaultMembershipProvider.NullDate;
              membership.FailedPasswordAttemptCount = 0;
              membership.FailedPasswordAttemptWindowStart = DefaultMembershipProvider.NullDate;
              membership.LastLockoutDate = DefaultMembershipProvider.NullDate;
            }
          }
          else
          {
            if (utcNow > membership.FailedPasswordAttemptWindowStart.AddMinutes((double) this.PasswordAttemptWindow))
              membership.FailedPasswordAttemptCount = 1;
            else
              ++membership.FailedPasswordAttemptCount;
            membership.FailedPasswordAttemptWindowStart = utcNow;
            if (membership.FailedPasswordAttemptCount >= this.MaxInvalidPasswordAttempts)
            {
              membership.IsLockedOut = true;
              membership.LastLockoutDate = utcNow;
            }
          }
          if (updateLastActivityDate)
          {
            membership.LastLoginDate = lastLoginDate;
            QueryHelper.GetUser(membershipEntities, membership.UserId, this.ApplicationName).LastActivityDate = lastActivityDate;
          }
          membershipEntities.SaveChanges();
        }
        return flag;
      }
    }

    private string GetEncodedPasswordAnswer(string userName, string passwordAnswer)
    {
      if (passwordAnswer != null)
        passwordAnswer = passwordAnswer.Trim();
      if (string.IsNullOrEmpty(passwordAnswer))
        return passwordAnswer;
      int status;
      string password;
      int format;
      string salt;
      int failedPasswordAttemptCount;
      int failedPasswordAnswerAttemptCount;
      bool isApproved;
      DateTime lastLoginDate;
      DateTime lastActivityDate;
      this.GetPasswordWithFormat(userName, false, out status, out password, out format, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
      DefaultMembershipProvider.ValidateStatus(status);
      return this.EncodePassword(passwordAnswer, format, salt);
    }

    private int Membership_CreateUser(string applicationName, string userName, string password, string salt, string email, string passwordQuestion, string passwordAnswer, bool isApproved, out DateTime createDate, bool uniqueEmail, int passwordFormat, ref object providerUserKey)
    {
      createDate = DateTime.UtcNow;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        Guid applicationId = (QueryHelper.GetApplication(membershipEntities, applicationName) ?? ModelHelper.CreateApplication(membershipEntities, applicationName)).ApplicationId;
        Guid? nullable1 = (Guid?) providerUserKey;
        User user = QueryHelper.GetUser(membershipEntities, userName, applicationName);
        Guid? nullable2 = user == null ? new Guid?() : new Guid?(user.UserId);
        bool flag;
        if (!nullable2.HasValue)
        {
          if (!nullable1.HasValue)
          {
            nullable2 = new Guid?(Guid.NewGuid());
          }
          else
          {
            Guid userId = nullable1.Value;
            if (QueryHelper.GetUser(membershipEntities, userId, applicationName) != null)
              return 10;
            nullable2 = new Guid?(nullable1.Value);
          }
          ModelHelper.CreateUser(membershipEntities, nullable2.Value, userName, applicationId, false);
          flag = true;
        }
        else
        {
          flag = false;
          if (nullable1.HasValue)
          {
            Guid? nullable3 = nullable2;
            Guid guid = nullable1.Value;
            if ((!nullable3.HasValue ? 1 : (nullable3.GetValueOrDefault() != guid ? 1 : 0)) != 0)
              return 6;
          }
        }
        if (QueryHelper.GetMembership(membershipEntities, applicationName, nullable2.Value) != null)
          return 6;
        if (uniqueEmail && QueryHelper.GetUserNameFromEmail(membershipEntities, email, applicationName) != null)
          return 7;
        if (!flag)
          user.LastActivityDate = createDate;
        MembershipEntity entity = new MembershipEntity();
        entity.ApplicationId = applicationId;
        entity.CreateDate = createDate;
        entity.Email = email;
        entity.FailedPasswordAnswerAttemptCount = 0;
        entity.FailedPasswordAnswerAttemptWindowsStart = DefaultMembershipProvider.NullDate;
        entity.FailedPasswordAttemptCount = 0;
        entity.FailedPasswordAttemptWindowStart = DefaultMembershipProvider.NullDate;
        entity.IsApproved = isApproved;
        entity.IsLockedOut = false;
        entity.LastLockoutDate = DefaultMembershipProvider.NullDate;
        entity.LastLoginDate = createDate;
        entity.LastPasswordChangedDate = createDate;
        entity.Password = password;
        entity.PasswordAnswer = passwordAnswer;
        entity.PasswordFormat = passwordFormat;
        entity.PasswordQuestion = passwordQuestion;
        entity.PasswordSalt = salt;
        entity.UserId = nullable2.Value;
        providerUserKey = (object) nullable2.Value;
        membershipEntities.Memberships.AddObject(entity);
        membershipEntities.SaveChanges();
        return 0;
      }
    }

    private void GetPasswordWithFormat(string userName, bool updateLastLoginActivityDate, out int status, out string password, out int format, out string salt, out int failedPasswordAttemptCount, out int failedPasswordAnswerAttemptCount, out bool isApproved, out DateTime lastLoginDate, out DateTime lastActivityDate)
    {
      password = (string) null;
      format = 0;
      salt = (string) null;
      failedPasswordAttemptCount = 0;
      failedPasswordAnswerAttemptCount = 0;
      isApproved = false;
      lastLoginDate = lastActivityDate = DateTime.UtcNow;
      status = 1;
      using (MembershipEntities membershipEntities = ModelHelper.CreateMembershipEntities(this.ConnectionString))
      {
        MembershipEntity membership = QueryHelper.GetMembership(membershipEntities, this.ApplicationName, userName);
        if (membership == null)
          return;
        if (membership.IsLockedOut)
        {
          status = 99;
        }
        else
        {
          password = membership.Password;
          format = membership.PasswordFormat;
          salt = membership.PasswordSalt;
          failedPasswordAttemptCount = membership.FailedPasswordAttemptCount;
          failedPasswordAnswerAttemptCount = membership.FailedPasswordAnswerAttemptCount;
          isApproved = membership.IsApproved;
          User user = QueryHelper.GetUser(membershipEntities, membership.UserId, this.ApplicationName);
          if (updateLastLoginActivityDate)
          {
            membership.LastLoginDate = user.LastActivityDate = DateTime.UtcNow;
            membershipEntities.SaveChanges();
          }
          lastLoginDate = membership.LastLoginDate;
          lastActivityDate = user.LastActivityDate;
          status = 0;
        }
      }
    }
  }
}
