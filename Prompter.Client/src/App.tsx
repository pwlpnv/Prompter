import { BrowserRouter, Routes, Route, NavLink } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { AllPromptsPage } from "./pages/AllPromptsPage";
import "./App.css";

function App() {
  return (
    <BrowserRouter>
      <nav className="navbar">
        <NavLink to="/" end>
          Home
        </NavLink>
        <NavLink to="/all">All Prompts</NavLink>
      </nav>
      <div className="container">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/all" element={<AllPromptsPage />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;
