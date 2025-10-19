INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt, IsActive, Name, PhoneNumber, PlanType, Country, IsRoaming)
VALUES 
('admin', 'admin@telecom.com', '$2a$11$example.hash.for.admin', 0, CURRENT_TIMESTAMP, TRUE, 'System Administrator', '+1234567890', 'Enterprise', 'US', FALSE),
('john.doe', 'john.doe@email.com', '$2a$11$example.hash.for.john', 1, CURRENT_TIMESTAMP, TRUE, 'John Doe', '+1234567891', 'Premium', 'US', FALSE),
('jane.smith', 'jane.smith@email.com', '$2a$11$example.hash.for.jane', 1, CURRENT_TIMESTAMP, TRUE, 'Jane Smith', '+1234567892', 'Basic', 'US', FALSE),
('bob.wilson', 'bob.wilson@email.com', '$2a$11$example.hash.for.bob', 1, CURRENT_TIMESTAMP, TRUE, 'Bob Wilson', '+1234567893', 'Premium', 'US', TRUE),
('alice.brown', 'alice.brown@email.com', '$2a$11$example.hash.for.alice', 1, CURRENT_TIMESTAMP, TRUE, 'Alice Brown', '+1234567894', 'Enterprise', 'US', FALSE);

INSERT INTO TariffRules (Name, PlanType, VoicePeakRate, VoiceOffPeakRate, DataRate, SMSRate, RoamingVoiceRate, RoamingDataRate, RoamingSMSRate, IsActive, CreatedAt)
VALUES 
('Basic Plan Tariff', 'Basic', 0.10, 0.05, 0.02, 0.15, 0.25, 0.08, 0.30, TRUE, CURRENT_TIMESTAMP),
('Premium Plan Tariff', 'Premium', 0.08, 0.04, 0.015, 0.12, 0.20, 0.06, 0.25, TRUE, CURRENT_TIMESTAMP),
('Enterprise Plan Tariff', 'Enterprise', 0.06, 0.03, 0.01, 0.10, 0.15, 0.04, 0.20, TRUE, CURRENT_TIMESTAMP);

INSERT INTO BundleLimits (PlanType, VoiceMinutesLimit, DataMBLimit, SMSLimit, PeakTimeMinutesLimit, OffPeakTimeMinutesLimit, IsActive, CreatedAt)
VALUES 
('Basic', 1000, 5000, 1000, 500, 500, TRUE, CURRENT_TIMESTAMP),
('Premium', 2000, 10000, 2000, 1000, 1000, TRUE, CURRENT_TIMESTAMP),
('Enterprise', 5000, 25000, 5000, 2500, 2500, TRUE, CURRENT_TIMESTAMP);

INSERT INTO UsageRecords (UserId, Timestamp, CallMinutes, DataMB, SMSCount, IsPeakTime, IsRoaming, CreatedAt)
VALUES 
(2, '2024-01-15 10:30:00', 120, 800, 45, TRUE, FALSE, CURRENT_TIMESTAMP),
(2, '2024-01-15 20:15:00', 80, 300, 25, FALSE, FALSE, CURRENT_TIMESTAMP),
(3, '2024-01-16 09:45:00', 200, 1200, 60, TRUE, FALSE, CURRENT_TIMESTAMP),
(3, '2024-01-16 19:30:00', 150, 600, 40, FALSE, FALSE, CURRENT_TIMESTAMP),
(4, '2024-01-17 11:20:00', 90, 500, 30, TRUE, TRUE, CURRENT_TIMESTAMP),
(4, '2024-01-17 21:10:00', 70, 400, 20, FALSE, TRUE, CURRENT_TIMESTAMP),
(5, '2024-01-18 08:00:00', 300, 2000, 100, TRUE, FALSE, CURRENT_TIMESTAMP),
(5, '2024-01-18 18:45:00', 250, 1500, 80, FALSE, FALSE, CURRENT_TIMESTAMP);

INSERT INTO Invoices (UserId, Month, BillingDate, TotalAmount, VoiceAmount, DataAmount, SMSAmount, RoamingAmount, VoiceMinutes, DataMB, SMSMessages, RoamingMinutes, RoamingDataMB, RoamingSMSMessages, CreatedAt)
VALUES 
(2, '2024-01', '2024-02-01', 45.50, 20.00, 16.00, 6.75, 2.75, 200, 1100, 70, 0, 0, 0, CURRENT_TIMESTAMP),
(3, '2024-01', '2024-02-01', 67.25, 28.00, 24.00, 12.00, 3.25, 350, 1800, 100, 0, 0, 0, CURRENT_TIMESTAMP),
(4, '2024-01', '2024-02-01', 52.80, 22.50, 18.00, 7.50, 4.80, 160, 900, 50, 90, 500, 30, CURRENT_TIMESTAMP),
(5, '2024-01', '2024-02-01', 125.00, 33.00, 35.00, 18.00, 39.00, 550, 3500, 180, 0, 0, 0, CURRENT_TIMESTAMP);
