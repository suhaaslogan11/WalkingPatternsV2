import { Routes, Route } from "react-router-dom";
import ClientList from "./pages/clients/ClientList";
import ClientForm from "./pages/clients/ClientForm";
import ProjectList from "./pages/projects/ProjectList";
import ProjectForm from "./pages/projects/ProjectForm";
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
