import { useState } from "react";
import "../Styles.css";
import { useNavigate } from "react-router-dom";

export default function Offers() {
    var AllData = JSON.parse(sessionStorage.getItem('Array')) || [];
    const [priceFilter, setPriceFilter] = useState(0);
    const [companyFilter, setCompanyFilter] = useState("");
    const [distanceFilter, setDistanceFilter] = useState(0);
    const [startDateFilter, setStartDateFilter] = useState("");
    const [endDateFilter, setEndDateFilter] = useState("");

    const [sortConfig, setSortConfig] = useState({ key: '', direction: 'ascending' });

    const navigate = useNavigate();
    const filteredData = AllData.filter(item => 
        item.Price >= priceFilter && 
        item.Distance >= distanceFilter &&
        (companyFilter === "" || item.CompanyName.toLowerCase().includes(companyFilter.toLowerCase())) &&
        (!startDateFilter || new Date(item.TravelStart) >= new Date(startDateFilter)) &&
        (!endDateFilter || new Date(item.TravelEnd) <= new Date(endDateFilter))
    );

    const sortedData = filteredData.sort((a, b) => {
        if (sortConfig.key) {
            const aValue = a[sortConfig.key];
            const bValue = b[sortConfig.key];

            if (typeof aValue === 'string') {
                return sortConfig.direction === 'ascending' 
                    ? aValue.localeCompare(bValue) 
                    : bValue.localeCompare(aValue);
            } else {
                return sortConfig.direction === 'ascending' 
                    ? aValue - bValue 
                    : bValue - aValue;
            }
        }
        return 0;
    });
    const requestSort = (key) => {
        let direction = 'ascending';
        if (sortConfig.key === key && sortConfig.direction === 'ascending') {
            direction = 'descending';
        }
        setSortConfig({ key, direction });
    };

    return (
        <div className="p-6 max-w-6xl mx-auto">
            <h1 className="text-2xl font-bold mb-4 text-center">Travel Deals</h1>
            <div className="filters">
                <div className="filter-item">
                    <input 
                        type="text" 
                        placeholder="Company Name" 
                        className="input-filter"
                        onChange={(e) => setCompanyFilter(e.target.value)}
                    />
                </div>
                <div className="filter-item">
                    <input 
                        type="number" 
                        placeholder="Price" 
                        className="input-filter"
                        onChange={(e) => setPriceFilter(Number(e.target.value))}
                    />
                </div>
                <div className="filter-item">
                    <input 
                        type="number" 
                        placeholder="Min Distance" 
                        className="input-filter"
                        onChange={(e) => setDistanceFilter(Number(e.target.value))}
                    />
                </div>
            </div>

            <div className="date-filters">
                <div className="date-filter-item">
                    <label className="filter-label">Start Date</label>
                    <input 
                        type="date" 
                        className="input-filter"
                        onChange={(e) => setStartDateFilter(e.target.value)}
                    />
                </div>
                <div className="date-filter-item">
                    <label className="filter-label">End Date</label>
                    <input 
                        type="date" 
                        className="input-filter"
                        onChange={(e) => setEndDateFilter(e.target.value)}
                    />
                </div>
            </div>



            
            <div className="text-center mt-4 mb-4">
                <button 
                    className="nav-btn"
                    onClick={() => navigate("/")}
                >
                    Homepage
                </button>
            </div>

            <table className="w-full border-collapse border border-gray-300">
                <thead>
                    <tr className="bg-gray-100">
                        <th className="border p-2" onClick={() => requestSort('FromPlanet')}>From</th>
                        <th className="border p-2" onClick={() => requestSort('ToPlanet')}>To</th>
                        <th className="border p-2" onClick={() => requestSort('CompanyName')}>Company</th>
                        <th className="border p-2" onClick={() => requestSort('Price')}>Price</th>
                        <th className="border p-2" onClick={() => requestSort('Distance')}>Distance</th>
                        <th className="border p-2" onClick={() => requestSort('TravelStart')}>Start Date</th>
                        <th className="border p-2" onClick={() => requestSort('TravelEnd')}>End Date</th>
                        <th className="border p-2">Reservation</th>
                    </tr>
                </thead>
                <tbody>
                {sortedData.map(item => (
                    <tr key={item.TravelId} className="border">
                        <td className="border p-2">{item.FromPlanet}</td>
                        <td className="border p-2">{item.ToPlanet}</td>
                        <td className="border p-2">{item.CompanyName}</td>
                        <td className="border p-2">${item.Price}</td>
                        <td className="border p-2">{item.Distance} km</td>
                        <td className="border p-2">{new Date(item.TravelStart).toLocaleDateString()}</td>
                        <td className="border p-2">{new Date(item.TravelEnd).toLocaleDateString()}</td>
                        <td className="border p-2">
                            <button className="reservation-btn"
                                onClick={() => {
                                    sessionStorage.setItem('SelectedTravel', JSON.stringify(item));
                                    navigate("/reservation");
                                }}>
                                Reservation
                            </button>
                        </td>
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    );
}
