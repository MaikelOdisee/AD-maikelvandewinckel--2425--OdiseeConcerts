using System.Collections.Generic;
using OdiseeConcerts.ViewModels; // Nodig voor ConcertViewModel

namespace OdiseeConcerts.Interfaces
{
    // Interface voor de Concert Service
    // Definieert de methoden die een Concert Service moet implementeren.
    public interface IConcertService
    {
        /// <summary>
        /// Haalt alle concerten op en converteert deze naar ConcertViewModel objecten.
        /// </summary>
        /// <returns>Een IEnumerable van ConcertViewModel objecten.</returns>
        IEnumerable<ConcertViewModel> GetAllConcerts();
    }
}
