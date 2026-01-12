USE [Guyton-Klinger-Withdrawals]
GO

/****** Object:  View [dbo].[vw_MostRecentBalances]    Script Date: 1/12/2026 2:01:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vw_MostRecentBalances]
AS
SELECT 
    [ID],
    [SequenceID],
    [Name],
    [Balance],
    [Type],
    [Error],
    [LastUpdate]
FROM (
    SELECT 
        [ID],
        [SequenceID],
        [Name],
        [Balance],
        [Type],
        [Error],
        [LastUpdate],
        -- Assign a rank of 1 to the newest record for each Name
        ROW_NUMBER() OVER (
            PARTITION BY [Name] 
            ORDER BY [LastUpdate] DESC, [ID] DESC
        ) AS RowNum
    FROM [dbo].[Balances]
) AS RankedData
WHERE RowNum = 1
AND [Balance] > 0.0;
GO

