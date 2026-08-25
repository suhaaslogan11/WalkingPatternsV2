import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import clientService from "../../services/clientService";
import type { Client } from "../../models/Client";

function ClientList() {

    const [clients, setClients] = useState<Client[]>([]);

    const loadClients = useCallback(async () => {

        try {

            const data = await clientService.getClients();

            setClients(data);

        }
        catch (error) {

            console.error(error);

            alert("Unable to load clients");

        }

    }, []);

    useEffect(() => {
        void clientService.getClients()
            .then(setClients)
            .catch((error) => {
                console.error(error);
                alert("Unable to load clients");
            });
    }, []);

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

        <div className="container py-3">

            <div>

                <div className="d-flex justify-content-end align-items-center border-bottom pb-2 mb-2">

                    <Link
                        to="/clients/add"
                        className="btn btn-primary">

                        Add Client

                    </Link>

                </div>

                <div>

                    <div className="table-responsive"><table className="table table-striped table-hover table-bordered mb-0">

                        <thead className="table-dark">

                            <tr>

                                <th>Id</th>
                                <th>Name</th>
                                <th>Phone</th>
                                <th>Email</th>
                                <th style={{ width: "230px" }}>
                                    Actions
                                </th>

                            </tr>

                        </thead>

                        <tbody>

                            {clients.length === 0 ? (

                                <tr>

                                    <td
                                        colSpan={5}
                                        className="empty-state">

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
                                                className="btn btn-outline-primary btn-sm me-2">

                                                Edit

                                            </Link>

                                            <Link
                                                to={`/clients/${client.clientId}/projects`}
                                                className="btn btn-primary btn-sm me-2">

                                                Projects

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

                    </table></div>

                </div>

            </div>

        </div>

    );

}

export default ClientList;
