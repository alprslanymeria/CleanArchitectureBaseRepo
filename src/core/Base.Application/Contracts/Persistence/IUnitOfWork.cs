namespace Base.Application.Contracts.Persistence;

// AMACIMIIZ DATABASE İŞLEMLERİNİ TEK BİR TRANSACTION ÜZERİNDEN GERÇEKLEŞTİRMEK.
public interface IUnitOfWork
{
    // TASK ASENKRON İŞLEMLERDE VOID'E KARŞILIK GELİR.
    Task<int> CommitAsync();
}