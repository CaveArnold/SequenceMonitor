USE [master]
GO

/****** Object:  Database [Guyton-Klinger-Withdrawals]    Script Date: 1/12/2026 1:59:20 PM ******/
CREATE DATABASE [Guyton-Klinger-Withdrawals]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Guyton-Klinger-Withdrawals', FILENAME = N'D:\My Documents\My Databases\Guyton-Klinger Withdrawals\Guyton-Klinger-Withdrawals.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Guyton-Klinger-Withdrawals_log', FILENAME = N'D:\My Documents\My Databases\Guyton-Klinger Withdrawals\Guyton-Klinger-Withdrawals_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Guyton-Klinger-Withdrawals].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET ARITHABORT OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET  DISABLE_BROKER 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET  MULTI_USER 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET DB_CHAINING OFF 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET QUERY_STORE = OFF
GO

ALTER DATABASE [Guyton-Klinger-Withdrawals] SET  READ_WRITE 
GO

