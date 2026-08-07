import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import clientService from "../../services/clientService";
import type { Client } from "../../models/Client";

function ClientList() {

    const [clients, setClients] = useState<Client[]>([]);

    useEffect(() => {
        loadClients();
    }, []);

    const loadClients = async () => {

        try {

            const data = await clientService.getClients();

            setClients(data);

        }
        catch (error) {

            console.error(error);

            alert("Unable to load clients");

        }

    };

    const handleDelete = async (id: number) => {

    const confirmDelete = window.confirm(
        "Are you sure you want to delete this client?"
    );

    if (!confirmDelete)
        return;

    try {

        await clientService.deleteClient(id);

        await loadClients();

        alert("Client deleted successfully.");

    }
    catch (error) {

        console.error(error);

        alert("Unable to delete client.");

    }

    };

    return (

        <div className="container mt-5">

            <div className="card shadow">

                <div className="card-header d-flex justify-content-between align-items-center">

                    <h3 className="mb-0">
                        Clients
                    </h3>

                    <Link
                        to="/clients/add"
                        className="btn btn-primary">

                        Add Client

                    </Link>

                </div>

                <div className="card-body">

                    <table className="table table-striped table-hover table-bordered">

                        <thead className="table-dark">

                            <tr>

                                <th>Id</th>
                                <th>Name</th>
                                <th>Phone</th>
                                <th>Email</th>
                                <th style={{ width: "150px" }}>
                                    Actions
                                </th>

                            </tr>

                        </thead>

                        <tbody>

                            {clients.length === 0 ? (

                                <tr>

                                    <td
                                        colSpan={5}
                                        className="text-center">

                                        No Clients Found

                                    </td>

                                </tr>

                            ) : (

                                clients.map((client) => (

                                    <tr key={client.clientId}>

                                        <td>{client.clientId}</td>
                                        <td>{client.clientName}</td>
                                        <td>{client.phone}</td>
                                        <td>{client.email}</td>

                                        <td>

                                            <Link
                                                to={`/clients/edit/${client.clientId}`}
                                                className="btn btn-warning btn-sm me-2">

                                                Edit

                                            </Link>

                                             <button className="btn btn-danger btn-sm" 
                                             onClick={() => handleDelete(client.clientId)}>
                                                Delete
                                                </button>

                                        </td>

                                    </tr>

                                ))

                            )}

                        </tbody>

                    </table>

                </div>

            </div>

        </div>

    );

}

export default ClientList;