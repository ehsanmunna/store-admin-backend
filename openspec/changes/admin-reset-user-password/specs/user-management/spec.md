## Purpose

Covers admin-facing management of existing users in the store-admin backend, starting with the ability for an admin to reset another existing user's password.

## ADDED Requirements

### Requirement: Admin Resets Existing User Password
The system SHALL allow a caller in the `Admin` role to reset the password of an existing user, identified by user id, to a new password supplied by the caller.

#### Scenario: Admin resets another user's password
- **WHEN** an authenticated `Admin` submits a new password for an existing user by that user's id
- **THEN** the system SHALL update that user's password to the new password
- **AND** the user SHALL be able to log in using the new password
- **AND** the user's previous password SHALL no longer be valid

#### Scenario: Target user does not exist
- **WHEN** an authenticated `Admin` submits a password reset for a user id that does not exist
- **THEN** the system SHALL reject the request without modifying any user's password

#### Scenario: New password fails validation rules
- **WHEN** an authenticated `Admin` submits a new password that does not satisfy the system's password rules
- **THEN** the system SHALL reject the request with a validation error
- **AND** the target user's existing password SHALL remain unchanged

### Requirement: Only Admin Role May Reset Passwords
The system SHALL restrict password reset of an existing user to callers in the `Admin` role. No self-service password reset and no reset by non-admin callers is provided.

#### Scenario: Non-admin caller attempts a password reset
- **WHEN** an authenticated caller who is not in the `Admin` role attempts to reset any user's password
- **THEN** the system SHALL reject the request
- **AND** the target user's existing password SHALL remain unchanged

#### Scenario: Unauthenticated caller attempts a password reset
- **WHEN** an unauthenticated caller attempts to reset any user's password
- **THEN** the system SHALL reject the request
- **AND** the target user's existing password SHALL remain unchanged
