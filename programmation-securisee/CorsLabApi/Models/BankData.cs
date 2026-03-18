namespace CorsLabApi.Models;

public record CompteDto(
    string Nom,
    string Iban,
    decimal Solde,
    string Devise
);

public record TransactionDto(
    string Date,
    string Libelle,
    decimal Montant,
    string Type
);
