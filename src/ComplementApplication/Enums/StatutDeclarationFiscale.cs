using System.ComponentModel.DataAnnotations;

namespace Enums;

// Example (Mollie API)
// https://docs.mollie.com/reference/create-payment
public enum StatutDeclarationFiscale
{
    // .resx file for translations
    [Display(Description = "Prête à être encodée")]
    PreteAEtreEncodée,
    [Display(Description = "Prête à être encodée")]
    PRETE_A_ETRE_ENCODEE,
    [Display(Description = "Sauvegardée en brouillon")]
    SauvegardéeEnBrouillon,
    Envoyée,
    AvertissementExtraitDeRoleDisponible,
    EnAttenteDeRemboursement,
    EnAttentedePaiement,
    Clôturée,
    Annulée
}