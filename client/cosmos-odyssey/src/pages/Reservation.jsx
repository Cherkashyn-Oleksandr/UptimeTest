import { useEffect, useState } from "react";
import "../Styles.css";
import { useNavigate } from "react-router-dom";

export default function Reservation() {
    const [travel, setTravel] = useState(null);
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    
    useEffect(() => {
        const data = sessionStorage.getItem("SelectedTravel");
        if (data) {
            setTravel(JSON.parse(data));
        }
    }, []);

    const handleReservation = async () => {
        if (!firstName || !lastName || !travel) {
            alert("Please fill in all fields.");
            return;
        }

        const reservationData = {
            ReservationID: crypto.randomUUID(),
            FirstName: firstName,
            LastName: lastName,
            TravelOfferID: travel.TravelId,
            PricelistId: travel.PricelistId,
            TravelPrice: travel.Price,
            TravelTime: Math.round((new Date(travel.TravelEnd) - new Date(travel.TravelStart)) / (1000 * 60 * 60)),
            CompanyName: travel.CompanyName,
            TravelStart: travel.TravelStart,
            TravelEnd: travel.TravelEnd
        };

        try {
            const response = await fetch("http://localhost:5078/api/CreateReservation",{
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(reservationData),
            });

            if (response.ok) {
                alert("Reservation successful!");
            } else {
                alert("Failed to reserve. Please try again.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("An error occurred. Please check the server.");
        }
    };

    const navigate = useNavigate();

    if (!travel) {
        return <div className="reservation-container">Loading travel info...</div>;
    }

    return (
        <div className="reservation-container">
            <h1 className="reservation-title">Reservation Details</h1>
            <div className="info-row"><span>From:</span><span>{travel.FromPlanet}</span></div>
            <div className="info-row"><span>To:</span><span>{travel.ToPlanet}</span></div>
            <div className="info-row"><span>Company:</span><span>{travel.CompanyName}</span></div>
            <div className="info-row"><span>Distance:</span><span>{travel.Distance} km</span></div>
            <div className="info-row"><span>Price:</span><span>${travel.Price}</span></div>
            <div className="info-row"><span>Start Date:</span><span>{new Date(travel.TravelStart).toLocaleDateString()}</span></div>
            <div className="info-row"><span>End Date:</span><span>{new Date(travel.TravelEnd).toLocaleDateString()}</span></div>

            <div className="input-group">
                <label>First Name</label>
                <input 
                    type="text"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    placeholder="Enter your first name"
                />
            </div>

            <div className="input-group">
                <label>Last Name</label>
                <input 
                    type="text"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    placeholder="Enter your last name"
                />
            </div>

            <button className="confirm-button" onClick={handleReservation}>Confirm</button>

            
            <div className="text-center mt-4">
                <button className="nav-btn"
                    onClick={() => navigate("/")}>
                    Homepage
                </button>
            </div>
        </div>
    );
}
