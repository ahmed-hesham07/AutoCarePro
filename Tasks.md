## Task Assignments

### Ahmed Hesham
1. Dashboard Form
   - Implement main dashboard with analytics
   - Create recommendations engine
   - Add priority-based alerts
   - Design maintenance history visualization

2. Maintenance History Form
   - Create detailed maintenance history view
   - Implement filtering and sorting
   - Add maintenance statistics

3. Recommendations Engine
   - Implement automated maintenance calculations
   - Create priority-based recommendation system
   - Add maintenance scheduling logic

4. Database Integration
   - Set up Entity Framework
   - Create database context
   - Implement data access layer

## Getting Started for Ahmed
1. Set up the database structure
   - Create Entity Framework models
   - Set up database context
   - Implement basic CRUD operations

2. Implement the Dashboard
   - Create main dashboard layout
   - Add analytics components
   - Implement recommendation engine

3. Create Maintenance History features
   - Implement detailed view
   - Add filtering and sorting
   - Create statistics calculations

---

### Nagham Mahmoud
1. Login/Registration Form
   - Create login interface
   - Add registration form
   - Implement password recovery

2. Car Information Form
   - Create form for basic car details
   - Add data validation
   - Implement basic CRUD operations

3. Basic Maintenance Entry Form
   - Create form for maintenance record entry
   - Add date and mileage inputs
   - Implement basic validation

---

## Detailed Guide for Nagham's Tasks

### 1. Login/Registration Form
#### Step-by-Step Guide:
1. **Create Basic Form Layout**
   - Open Visual Studio
   - Right-click on Forms folder → Add → Windows Form
   - Name it "LoginForm"
   - Add these controls using the Toolbox:
     - TextBox for username
     - TextBox for password (set PasswordChar to '*')
     - Button for login
     - Button for registration
     - LinkLabel for "Forgot Password"

2. **Add Form Design**
   - Set form properties:
     - FormBorderStyle: FixedSingle
     - StartPosition: CenterScreen
     - Text: "AutoCarePro Login"
   - Add a PictureBox for logo
   - Use TableLayoutPanel for better control arrangement

3. **Implement Basic Validation**
   - Add TextChanged event for username/password
   - Check for empty fields
   - Show error messages using MessageBox

#### Learning Resources:
- [Windows Forms Tutorial](https://www.tutorialspoint.com/csharp/csharp_windows_forms.htm)
- [Form Controls Guide](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/?view=netdesktop-6.0)
- [Form Validation](https://www.c-sharpcorner.com/article/form-validation-in-windows-form-application/)

### 2. Car Information Form
#### Step-by-Step Guide:
1. **Create Form Layout**
   - Add new Windows Form named "CarInfoForm"
   - Add these controls:
     - TextBox for Make
     - TextBox for Model
     - NumericUpDown for Year
     - TextBox for VIN
     - NumericUpDown for Mileage
     - ComboBox for Fuel Type
     - Button for Save
     - Button for Cancel

2. **Add Data Validation**
   - Validate VIN format
   - Check year range (1900-current year)
   - Ensure mileage is positive
   - Make required fields mandatory

3. **Implement Basic CRUD**
   - Add "New Car" button
   - Add "Edit" button
   - Add "Delete" button
   - Add DataGridView to show car list

#### Learning Resources:
- [DataGridView Tutorial](https://www.c-sharpcorner.com/UploadFile/mahesh/datagridview-in-C-Sharp/)
- [Form Validation Examples](https://www.c-sharpcorner.com/article/form-validation-in-windows-form-application/)
- [ComboBox Usage](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-add-and-remove-items-from-a-windows-forms-combobox-listbox-or-checkedlistbox-control?view=netdesktop-6.0)

### 3. Maintenance Entry Form
#### Step-by-Step Guide:
1. **Create Form Layout**
   - Add new Windows Form named "MaintenanceEntryForm"
   - Add these controls:
     - DateTimePicker for maintenance date
     - NumericUpDown for mileage
     - ComboBox for maintenance type
     - TextBox for description
     - NumericUpDown for cost
     - TextBox for service provider
     - TextBox for notes
     - Button for Save
     - Button for Cancel

2. **Add Basic Validation**
   - Check date is not in future
   - Ensure mileage is positive
   - Make required fields mandatory
   - Validate cost is positive

3. **Implement Basic Features**
   - Add maintenance type list
   - Add date validation
   - Add basic error handling

#### Learning Resources:
- [DateTimePicker Guide](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-set-and-return-dates-with-the-windows-forms-datetimepicker-control?view=netdesktop-6.0)
- [NumericUpDown Usage](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-set-the-format-for-the-windows-forms-numericupdown-control?view=netdesktop-6.0)
- [Error Handling](https://www.c-sharpcorner.com/article/error-handling-in-windows-forms-application/)

### General Tips for Nagham:
1. **Using Visual Studio:**
   - Use the Toolbox (View → Toolbox)
   - Use the Properties window (View → Properties)
   - Use the Solution Explorer (View → Solution Explorer)

2. **Debugging:**
   - Use MessageBox.Show() to check values
   - Use breakpoints (F9) to pause code
   - Use F5 to run the application

3. **Getting Help:**
   - Use Visual Studio's IntelliSense (Ctrl+Space)
   - Search on Stack Overflow
   - Check Microsoft Documentation
   - Ask Ahmed for help with complex issues

4. **Best Practices:**
   - Comment your code
   - Use meaningful variable names
   - Test your forms after each change
   - Save your work frequently

### Useful Websites:
1. [C# Corner](https://www.c-sharpcorner.com/)
2. [Tutorials Point](https://www.tutorialspoint.com/csharp/index.htm)
3. [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/?view=netdesktop-6.0)
4. [Stack Overflow](https://stackoverflow.com/questions/tagged/c%23+winforms)

### YouTube Tutorials:
1. [Windows Forms Basics](https://www.youtube.com/watch?v=7m7X_8aFw-o)
2. [Form Design](https://www.youtube.com/watch?v=7m7X_8aFw-o)
3. [Data Validation](https://www.youtube.com/watch?v=7m7X_8aFw-o) 