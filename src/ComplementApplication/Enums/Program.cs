// See https://aka.ms/new-console-template for more information

using Enums;

Console.WriteLine("Hello, World!");

var declarationFiscale = new DeclarationFiscale()
{
    NationalNumber = "123456789",
    Year = 2025,
    Status = StatutDeclarationFiscale.PreteAEtreEncodée
};

var declarationFiscale2 = new DeclarationFiscale()
{
    NationalNumber = "987654321",
    Year = 2025,
    Status = StatutDeclarationFiscale.EnAttenteDeRemboursement
};