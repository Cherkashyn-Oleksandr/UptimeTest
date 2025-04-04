import { createBrowserRouter,RouterProvider,Route,Outlet } from "react-router-dom";
import Home from "./pages/Home"
import Offers from "./pages/Offers"
import Reservation from "./pages/Reservation"

const Layout =()=> {
  return(
    <>
    <Outlet></Outlet>
    </>
  );
};

const router = createBrowserRouter([
  {
    path: "/",
    element:<Layout/>,
    children:[
      {
        path:"/",
        element:<Home/>
      },
      {
        path:"/offers",
        element:<Offers/>
      },
      {
        path:"/reservation",
        element:<Reservation/>
      },
    ]
  },
]);

function App() {
  return (
    <div className="App">
      <div className="container">
        <RouterProvider router={router}/>
      </div>
    </div>
  );
}

export default App;