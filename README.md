# AutoCarePro - Vehicle Maintenance Tracker

## Project Overview
AutoCarePro is a desktop application designed to help car owners and maintenance centers track vehicle maintenance schedules, expenses, and diagnostics. The application provides automated maintenance recommendations and priority-based alerts to ensure vehicles are properly maintained.

## Features
- User authentication (Car Owners and Maintenance Centers)
- Vehicle management and tracking
- Maintenance history recording
- Automated maintenance recommendations
- Priority-based alerts
- Maintenance cost tracking
- Service provider management

## Project Structure
```
AutoCarePro/
├── Models/
│   ├── Vehicle.cs (Base class)
│   ├── Car.cs (Inherits from Vehicle)
│   ├── MaintenanceRecord.cs
│   ├── MaintenanceRecommendation.cs
│   └── User.cs
├── Forms/
│   ├── LoginForm.cs
│   ├── CarInfoForm.cs
│   ├── MaintenanceEntryForm.cs
│   ├── DashboardForm.cs
│   └── MaintenanceHistoryForm.cs
└── Services/
    ├── DatabaseService.cs
    └── RecommendationEngine.cs
```

## Workflow Chart
```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Application Start                           │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           Login/Registration                             │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                               Dashboard                                  │
└───────────────┬───────────────┬───────────────┬─────────────────────────┘
                │               │               │
                ▼               ▼               ▼
┌───────────────┴───┐   ┌───────┴───────┐   ┌──┴───────────────┐
│   Vehicle         │   │  Maintenance  │   │  Recommendations │
│   Management      │   │  History      │   │  & Alerts        │
└─────────┬─────────┘   └───────┬───────┘   └────────┬─────────┘
          │                     │                    │
          ▼                     ▼                    ▼
┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
│ Add/Edit        │   │ View/Filter     │   │ View Priority   │
│ Vehicle Details │   │ Maintenance     │   │ Alerts          │
└─────────────────┘   │ Records         │   └─────────────────┘
                      └─────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        Maintenance Workflow                              │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        Check Maintenance Status                          │
└───────────────┬───────────────────────────────┬─────────────────────────┘
                │                               │
                ▼                               ▼
┌─────────────────────────┐         ┌─────────────────────────┐
│ Maintenance Due         │         │ No Maintenance Due      │
└───────────────┬─────────┘         └─────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        Generate Recommendations                          │
└───────────────┬───────────────────────────────┬─────────────────────────┘
                │                               │
                ▼                               ▼
┌─────────────────────────┐         ┌─────────────────────────┐
│ Critical Priority       │         │ Regular Priority        │
└───────────────┬─────────┘         └───────────────┬─────────┘
                │                                   │
                ▼                                   ▼
┌─────────────────────────┐         ┌─────────────────────────┐
│ Send Alert              │         │ Add to Schedule         │
└─────────────────────────┘         └─────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        User Type Workflows                               │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │
                ┌───────────────┴───────────────┐
                │                               │
                ▼                               ▼
┌─────────────────────────┐         ┌─────────────────────────┐
│ Car Owner               │         │ Maintenance Center      │
└───────────────┬─────────┘         └───────────────┬─────────┘
                │                                   │
                ▼                                   ▼
┌─────────────────────────┐         ┌─────────────────────────┐
│ View Vehicle Status     │         │ Manage Service Records  │
│ Track Maintenance       │         │ Update Maintenance      │
│ Receive Alerts          │         │ Generate Reports        │
└─────────────────────────┘         └─────────────────────────┘
```

## Setup Instructions
1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the solution
5. Run the application

## Development Guidelines
- Use C# coding conventions
- Follow SOLID principles
- Document code with XML comments
- Write meaningful commit messages
- Test features before committing

## Communication
- Use GitHub Issues for bug tracking
- Create pull requests for code review
- Document major changes in commit messages
- Keep the team updated on progress

## Important Notes
- Always pull latest changes before starting work
- Test your changes before committing
- Ask for help if stuck
- Keep code clean and well-documented

## Resources
- [C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Windows Forms Guide](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/?view=netdesktop-6.0)
- [Entity Framework Documentation](https://docs.microsoft.com/en-us/ef/)

## Contact
- Ahmed Hesham: [Your Contact Information]
- Nagham Mahmoud: [Nagham's Contact Information]

