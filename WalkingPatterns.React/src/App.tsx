import { Navigate, Routes, Route } from "react-router-dom";
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
import Login from "./pages/Login";
import ProtectedRoute from "./components/ProtectedRoute";
import InactivityLogout from "./components/InactivityLogout";

function App() {

    return (
    <>
        <InactivityLogout />
        <Routes>
            <Route path="/login" element={<Login />} />
            <Route element={<ProtectedRoute />}>

            <Route
                path="/"
                element={<Navigate to="/clients" replace />}
            />

            <Route
                path="/clients"
                element={<ClientList />}
            />

            <Route
                path="/clients/new"
                element={<ClientForm />}
            />

            <Route
                path="/clients/add"
                element={<ClientForm />}
            />

            <Route
                path="/clients/:id/edit"
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
                path="/projects/:projectId/edit"
                element={<ProjectForm />}
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

            <Route path="*" element={<Navigate to="/clients" replace />} />
            </Route>

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
