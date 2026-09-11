namespace AssetFlow.Common.Settings
{
    public class EmailSettings 
    {
        public string EmailSubject { get; set; }
        public string ResetPasswordURL { get; set; }
        public string ResetPasswordURLDeepLink { get; set; }

        public string UserEmail { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }

        public string CompanyLogo { get; set; }
        public string Shadow { get; set; }
        public string SecondShadow { get; set; }
        public string BottomShadow { get; set; }

        public string HtmlTemplatePath { get; set; }
        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }
    }
}
