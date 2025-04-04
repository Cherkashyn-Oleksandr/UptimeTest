import React, { useState } from "react";
import "../Styles.css";
import { useNavigate } from "react-router-dom";

const Home = () => {
  const planets = [
    "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune"
  ];

  const navigate = useNavigate()
  //array for accesible routes
  const routes = { 
    mercury: ["venus"],
    venus: ["mercury", "earth"],
    earth: ["jupiter", "uranus"],
    mars: ["venus"],
    jupiter: ["venus", "mars"],
    saturn: ["neptune", "earth"],
    uranus: ["saturn", "neptune"],
    neptune: ["mercury", "uranus"]
  };

  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");

  // Change From input
  const handleFromChange = (e) => {
    const selectedFrom = e.target.value;
    setFrom(selectedFrom);
    if (!to) {
      setTo(""); 
    }
  };

  // Change To input
  const handleToChange = (e) => {
    setTo(e.target.value);
  };

  // accessible To inputs
  const accessTo = from ? routes[from.toLowerCase()] || [] : planets;

  // accessible From inputs
  const accessFrom = to ? planets.filter(planet => routes[planet.toLowerCase()]?.includes(to.toLowerCase())) : planets;

  // Get offers & Update new pricelists
  const handleCheckRoutes = async () => {
    if (from && to) {
      const url = `http://localhost:5078/api/TravelOffers/${from.toLowerCase()}/${to.toLowerCase()}`;

      try {
        const res = await fetch(url);
        const data = await res.json();
        sessionStorage.setItem('Array', JSON.stringify(data));
        //sometimes get UpdatePriceList takes a lot time cua of free and slow database
        const updatePriceListUrl = "http://localhost:5078/api/UpdatePriceList";  
        await fetch(updatePriceListUrl, {
          method: "GET", 
        });
        navigate("/offers");
      } catch (error) {
        console.error("Error:", error);
      }
    } else {
      console.error("Please select both 'from' and 'to' planets.");
    }
  };

  return (
    <div>
      <h1 className="title">Select Your Route</h1>
      <div className="form-container">
        <div className="input-row">
          <div className="input-group">
            <label htmlFor="from">From</label>
            <select
              id="from"
              className="select-input"
              value={from}
              onChange={handleFromChange}
            >
              <option value="">Select a planet</option>
              {accessFrom.map((planet, index) => (
                <option key={index} value={planet.toLowerCase()}>
                  {planet.charAt(0).toUpperCase() + planet.slice(1)}
                </option>
              ))}
            </select>
          </div>
          <div className="input-group">
            <label htmlFor="to">To</label>
            <select
              id="to"
              className="select-input"
              value={to}
              onChange={handleToChange}
            >
              <option value="">Select a planet</option>
              {accessTo.length > 0
                ? accessTo.map((planet, index) => (
                    <option key={index} value={planet.toLowerCase()}>
                      {planet.charAt(0).toUpperCase() + planet.slice(1)}
                    </option>
                  ))
                : <option value="">No available planets</option>}
            </select>
          </div>
        </div>
        <button className="nav-btn" onClick={handleCheckRoutes}>Check Routes</button>
      </div>
    </div>
  );
};

export default Home;
