USE [Guyton-Klinger-Withdrawals]
GO

/****** Object:  Table [dbo].[Balances]    Script Date: 1/12/2026 1:59:51 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Balances](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SequenceID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Balance] [money] NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Error] [nvarchar](256) NULL,
	[LastUpdate] [datetime] NULL,
 CONSTRAINT [PK_Balances] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

