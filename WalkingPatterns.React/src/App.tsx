import { Routes, Route } from "react-router-dom";
import ClientList from "./pages/clients/ClientList";
import ClientForm from "./pages/clients/ClientForm";
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