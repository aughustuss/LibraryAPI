namespace LibraryAPI.Helpers
{
    public class EmailBody
    {
        public static string EmailStrinBody(string email, string emailToken)
        {
            return $@"
<html lang=""pt-br"">

<head>
</head>
<link rel=""preconnect"" href=""https://fonts.googleapis.com"">
<link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
<link href=""https://fonts.googleapis.com/css2?family=Inter&display=swap"" rel=""stylesheet"">
<body>
  <div stlye=""display: flex; flex-direction: column; justify-content: center; align-content: center; align-items: center; gap: 20px; font-family: 'Inter', sans-serif;"">
    <h1 style=""font-weight: 800"">
    Confirme sua conta.
  </h1>
    <p>
      Confirme sua conta clicando no botão abaixo 
    </p>
  <a href=""http://localhost:4200/confirmAccount?email={email}&code={emailToken}"" target=""_blank"" style=""padding: 10px; border:none; background-color:#116149; border-radius: 5px; color: white; text-decoration:none; font-size: 12px;"">
    Confirme sua Conta
  </a>
    <p>
      ou clique no link abaixo: 
      <p/>
    <a href=""http://localhost:4200/confirmAccount?email={{email}}&code={{emailToken"">
      http://localhost:4200/confirmAccount?email={{email}}&code={{emailToken
    </a>
  </div>
</body>
</html>

";
        }
    }
}
