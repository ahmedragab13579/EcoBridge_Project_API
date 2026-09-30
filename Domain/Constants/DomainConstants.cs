using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Domain.Constants
{
    public class DomainConstants
    {
        public static class Financial
        {
            public const decimal MaxAbsAmount = 1_000_000m;

        }


        public static class Name
        {
            public const int MaxNameLength = 100;
            public const int MaxCodeLength = 50;


        }


        public static class Security
        {
            public const int MaxTokenLength = 512;
            public const int MaxRevocationReasonLength = 200;
            public const string RotationReason = "Rotated.";
            public static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(7);

        }



        public static class MissionVal
        {
            public const int OtpLength = 6;
            public const int MaxOtpAttempts = 5;
            public static readonly TimeSpan DefaultOtpLifetime = TimeSpan.FromMinutes(15);
        }

        public static class Body
        {
            public const int MaxBodyLength = 1000;
            public const int MaxLocationLength = 255;
            public const int MaxEmailLength = 255;
            public const int MaxPhoneLength = 20;
            public const int MaxJobLength = 20;


        }




        public static class DonationVal
        {
            public const int MaxFoodTypeLength = 100;
            public const int MaxQuantityLength = 100;
            public const int MaxPickupLocationLength = 200;
            public const int MaxCancellationReasonLength = 200;
            public const decimal MaxWeightKg = 1000m;
        }

        public static class Lengths
        {
            public const int MaxReasonCodeLength = 100;
            public const int MaxReasonRatingNumber = 5;
            public const int MinReasonRatingNumber = 1;

        }



        public static class Score
        {
           public const int MaxTrustScore = 100;
           public const int MinTrustScore = 0;
           public const int InitialTrustScore = 100;
           public const int InitialWalletPoints = 0;
           public const int MaxWalletPoints = 1_000_000;
        }

        public static class Media
        {
            public const int MaxImageUrlLength = 500;
        }
    }
}
