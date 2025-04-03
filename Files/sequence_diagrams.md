# Sequence Diagrams for RoomieBill

## Use Case 1: Adding a Shared Expense
```mermaid
sequenceDiagram
    actor User
    participant System
    
    User->>System: Select Add Expense
    System->>User: Display Expense Form
    User->>System: Enter expense details (amount, description, category)
    User->>System: Select group members to split with
    User->>System: Confirm expense
    System->>User: Display expense confirmation
    System->>User: Update balances display
```

## Use Case 2: Tracking and Logging a Payment
```mermaid
sequenceDiagram
    actor Payer
    participant System
    actor Recipient
    
    Payer->>System: Select Make Payment
    System->>Payer: Display payment form
    Payer->>System: Enter payment amount
    Payer->>System: Select recipient
    Payer->>System: Confirm payment
    System->>Recipient: Send payment notification
    System->>Payer: Update balance
    System->>Recipient: Update balance
```

## Use Case 3: Automating a Recurring Expense
```mermaid
sequenceDiagram
    actor User
    participant System
    
    User->>System: Select Set Recurring Expense
    System->>User: Display recurring expense form
    User->>System: Enter expense details
    User->>System: Set recurrence pattern (daily/weekly/monthly)
    User->>System: Select participants
    User->>System: Confirm setup
    System->>User: Display confirmation
    Note over System: System will automatically create expense entries based on pattern
```

## Use Case 4: Generating a Summary
```mermaid
sequenceDiagram
    actor User
    participant System
    
    User->>System: Request Summary
    System->>User: Display summary options
    User->>System: Select time period
    User->>System: Select summary type (expenses/payments/balances)
    System->>User: Generate and display summary
    User->>System: Optional: Request to export
    System->>User: Provide exported summary
```

## Use Case 5: Sending a Payment Reminder
```mermaid
sequenceDiagram
    actor Creditor
    participant System
    actor Debtor
    
    Creditor->>System: Select Send Reminder
    System->>Creditor: Show pending payments
    Creditor->>System: Select debtor and amount
    Creditor->>System: Confirm reminder
    System->>Debtor: Send payment reminder notification
    System->>Creditor: Confirm reminder sent
    Note over Debtor: Receives notification
```

## Use Case 6: Viewing Expense History
```mermaid
sequenceDiagram
    actor User
    participant System
    
    User->>System: Access Expense History
    System->>User: Display filter options
    User->>System: Optional: Set date range
    User->>System: Optional: Select categories
    User->>System: Optional: Select group members
    System->>User: Display filtered expense history
    User->>System: Optional: Sort/Filter results
    System->>User: Update display
```

## Use Case 7: Proposing a New Expense Split
```mermaid
sequenceDiagram
    actor Proposer
    participant System
    actor GroupMembers
    
    Proposer->>System: Create new expense split
    System->>Proposer: Display split options
    Proposer->>System: Enter expense details
    Proposer->>System: Set custom split ratios
    Proposer->>System: Submit proposal
    System->>GroupMembers: Send split proposal notification
    GroupMembers->>System: Review and respond
    System->>Proposer: Update proposal status
```

## Use Case 8: Monthly Budget Overview
```mermaid
sequenceDiagram
    actor User
    participant System
    
    User->>System: Request Budget Overview
    System->>User: Display month selection
    User->>System: Select month
    System->>User: Show expenses by category
    System->>User: Display spending trends
    System->>User: Show budget status
    User->>System: Optional: Adjust budget
    System->>User: Update budget display
```

## Use Case 9: Group Management
```mermaid
sequenceDiagram
    actor Admin
    participant System
    actor Members
    
    Admin->>System: Access Group Settings
    System->>Admin: Display group options
    Admin->>System: Modify group settings
    Admin->>System: Add/Remove members
    System->>Members: Send group update notification
    System->>Admin: Confirm changes
    System->>Members: Update group view
