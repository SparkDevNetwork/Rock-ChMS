IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonCurrent]', 'V') IS NOT NULL
    DROP VIEW [dbo].AnalyticsDimPersonCurrent
GO

CREATE VIEW [dbo].AnalyticsDimPersonCurrent
AS
SELECT * FROM AnalyticsDimPersonHistorical where CurrentRowIndicator = 1