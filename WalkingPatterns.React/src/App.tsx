import { Routes, Route } from "react-router-dom";
import ClientList from "./pages/clients/ClientList";
import ClientForm from "./pages/clients/ClientForm";
import ProjectList from "./pages/projects/ProjectList";
import ProjectForm from "./pages/projects/ProjectForm";
import ProjectDetail from "./pages/projects/ProjectDetail";
import KitchenPricing from "./pages/projects/KitchenPricing";
import BedroomPricing from "./pages/projects/BedroomPricing";
import OtherWoodworkPricing from "./pages/projects/OtherWoodworkPricing";
import HdsPricing from "./pages/projects/HdsPricing";
import { ToastContainer } from "react-toastify";

function App() {

    return (
    <>
        <Routes>

            <Route
                path="/"
                element={<ClientList />}
            />

            <Route
                path="/clients/add"
                element={<ClientForm />}
            />

            <Route
                path="/clients/edit/:id"
                element={<ClientForm />}
            />

            <Route
                path="/clients/:clientId/projects"
                element={<ProjectList />}
            />

            <Route
                path="/projects/edit/:id"
                element={<ProjectForm />}
            />

            <Route
                path="/projects/:projectId"
                element={<ProjectDetail />}
            />

            <Route
                path="/projects/:projectId/kitchen"
                element={<KitchenPricing />}
            />

            <Route
                path="/projects/:projectId/bedroom"
                element={<BedroomPricing />}
            />

            <Route
                path="/projects/:projectId/other-woodwork"
                element={<OtherWoodworkPricing />}
            />

            <Route
                path="/projects/:projectId/hds"
                element={<HdsPricing />}
            />

        </Routes>

        <ToastContainer
            position="top-right"
            autoClose={3000}
            hideProgressBar={false}
            newestOnTop
            closeOnClick
            pauseOnHover
            theme="colored"
        />
    </>
);



}

export default App;
