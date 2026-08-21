## Purpose

Lets new accounts be created through a public registration flow, and bootstraps the very first account as an administrator so the system needs no pre-seeded admin credentials to become usable.

## ADDED Requirements

### Requirement: Public registration endpoint
The system SHALL provide a public registration endpoint that creates a new user account given a first name, last name, email, and password.

#### Scenario: Successful registration
- **WHEN** a client submits a valid first name, last name, email, and password to the registration endpoint
- **THEN** the system creates a new user account and returns an authenticated response (token and roles) for that account

#### Scenario: Duplicate email rejected
- **WHEN** a client submits a registration with an email that already belongs to an existing account
- **THEN** the system rejects the request with an error and does not create a duplicate account

### Requirement: First registered account becomes Admin
The system SHALL assign the Admin role to a newly registered account when no existing account currently holds the Admin role.

#### Scenario: Bootstrap first admin
- **WHEN** a user registers and no existing account holds the Admin role
- **THEN** the newly created account SHALL be assigned the Admin role

### Requirement: Subsequent registrations default to Customer
The system SHALL assign the Customer role to a newly registered account whenever at least one existing account already holds the Admin role.

#### Scenario: Registration after an admin exists
- **WHEN** a user registers and at least one existing account already holds the Admin role
- **THEN** the newly created account SHALL be assigned the Customer role

### Requirement: No automatic account seeding on startup
The system SHALL NOT automatically create or promote any user account as a side effect of application startup.

#### Scenario: Startup does not seed a user
- **WHEN** the application starts
- **THEN** no user account is created, promoted, or otherwise modified as a result of startup

### Requirement: Public registration page
The admin frontend SHALL provide a registration page reachable without authentication that submits a first name, last name, email, and password to the registration endpoint.

#### Scenario: User completes registration through the UI
- **WHEN** a visitor fills out and submits the registration form with valid details
- **THEN** the frontend calls the registration endpoint and, on success, stores the returned session and navigates into the app the same way a successful login does

#### Scenario: Registration error is shown
- **WHEN** the registration endpoint rejects the submission (e.g. duplicate email)
- **THEN** the frontend displays the error to the user without navigating away from the registration page
