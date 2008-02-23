using System;
using System.Net.Mail;
using DotNetOpenId.Provider;
using DotNetOpenId.RegistrationExtension;

/// <summary>
/// Handles the collection of the simple registration fields.
/// Only mandatory or optional fields are displayed. Mandatory fields have a '*' next to them.
/// No validation occurs here.
/// </summary>
public partial class ProfileFields : System.Web.UI.UserControl
{
	protected void Page_Load(object sender, EventArgs e) {
	}

	public void SetRequiredFieldsFromRequest(ProfileRequestFields requestFields) {
		dobRequiredLabel.Visible = (requestFields.Birthdate == ProfileRequest.Require);
		countryRequiredLabel.Visible = (requestFields.Country == ProfileRequest.Require);
		emailRequiredLabel.Visible = (requestFields.Email == ProfileRequest.Require);
		fullnameRequiredLabel.Visible = (requestFields.FullName == ProfileRequest.Require);
		genderRequiredLabel.Visible = (requestFields.Gender == ProfileRequest.Require);
		languageRequiredLabel.Visible = (requestFields.Language == ProfileRequest.Require);
		nicknameRequiredLabel.Visible = (requestFields.Nickname == ProfileRequest.Require);
		postcodeRequiredLabel.Visible = (requestFields.PostalCode == ProfileRequest.Require);
		timezoneRequiredLabel.Visible = (requestFields.TimeZone == ProfileRequest.Require);

		dateOfBirthRow.Visible = !(requestFields.Birthdate == ProfileRequest.NoRequest);
		countryRow.Visible = !(requestFields.Country == ProfileRequest.NoRequest);
		emailRow.Visible = !(requestFields.Email == ProfileRequest.NoRequest);
		fullnameRow.Visible = !(requestFields.FullName == ProfileRequest.NoRequest);
		genderRow.Visible = !(requestFields.Gender == ProfileRequest.NoRequest);
		languageRow.Visible = !(requestFields.Language == ProfileRequest.NoRequest);
		nicknameRow.Visible = !(requestFields.Nickname == ProfileRequest.NoRequest);
		postcodeRow.Visible = !(requestFields.PostalCode == ProfileRequest.NoRequest);
		timezoneRow.Visible = !(requestFields.TimeZone == ProfileRequest.NoRequest);
	}
    
    public bool DoesAnyFieldHaveAValue
    {
        get
        {
            return  !((DateOfBirth == null)
            && String.IsNullOrEmpty(countryDropdownList.SelectedValue)
            && String.IsNullOrEmpty(emailTextBox.Text)
            && String.IsNullOrEmpty(fullnameTextBox.Text)
            && (Gender == null)
            && String.IsNullOrEmpty(languageDropdownList.SelectedValue)
            && String.IsNullOrEmpty(nicknameTextBox.Text)
            && String.IsNullOrEmpty(postcodeTextBox.Text)
            && String.IsNullOrEmpty(timezoneDropdownList.SelectedValue));
        }
    }
    
    public DateTime ?DateOfBirth
    {
        get
        {
            try
            {
                int day = Convert.ToInt32(dobDayDropdownlist.SelectedValue);
                int month = Convert.ToInt32(dobMonthDropdownlist.SelectedValue);
                int year = Convert.ToInt32(dobYearDropdownlist.SelectedValue);
                DateTime newDate = new DateTime(year, month, day);
                return newDate;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    
    public Gender ?Gender
    {
        get
        {
            if (this.genderDropdownList.SelectedValue == "Male")
            {
                return DotNetOpenId.RegistrationExtension.Gender.Male;
            }
            if (this.genderDropdownList.SelectedValue == "Female")
            {
                return DotNetOpenId.RegistrationExtension.Gender.Female;
            }
            return null;
        }
    }

    public ProfileFieldValues OpenIdProfileFields
    {
        get
        {
            ProfileFieldValues fields = new ProfileFieldValues();
            fields.Birthdate = DateOfBirth;
            fields.Country = countryDropdownList.SelectedValue;
            fields.Email = emailTextBox.Text;
            fields.Fullname = fullnameTextBox.Text;
            fields.Gender = Gender;
            fields.Language = languageDropdownList.SelectedValue;
            fields.Nickname = nicknameTextBox.Text;
            fields.PostalCode = postcodeTextBox.Text;
            fields.TimeZone = timezoneDropdownList.SelectedValue;
            return fields;
        }
    }
    
}