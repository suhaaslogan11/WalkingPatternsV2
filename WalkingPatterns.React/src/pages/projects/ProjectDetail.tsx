import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify";
import type { ProjectDetailPage, ProjectOrders } from "../../models/Project";
import projectService from "../../services/projectService";

function ProjectDetail() {

    const navigate = useNavigate();
    const { projectId } = useParams();
    const parsedProjectId = Number(projectId);
    const [details, setDetails] = useState<ProjectDetailPage>();
    const [orders, setOrders] = useState<ProjectOrders | null>(null);

    useEffect(() => {

        if (!Number.isInteger(parsedProjectId) || parsedProjectId <= 0)
            return;

        void projectService.getProjectDetails(parsedProjectId)
            .then(setDetails)
            .catch((error) => {
                console.error(error);
                toast.error("Unable to load project details");
            });

    }, [parsedProjectId]);

    const handleViewOrders = async (projectDetailId: number) => {

        try {

            const data = await projectService.getProjectDetailOrders(projectDetailId);
            setOrders(data);

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to load orders");

        }

    };

    if (!Number.isInteger(parsedProjectId) || parsedProjectId <= 0) {
        return (
            <div className="container mt-5">
                <div className="alert alert-danger">Invalid project.</div>
                <button className="btn btn-secondary" onClick={() => navigate(-1)}>
                    Back
                </button>
            </div>
        );
    }

    if (!details) {
        return <div className="container mt-5">Loading project details...</div>;
    }

    return (

        <div className="container mt-5">

            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2 className="mb-0">Project Details</h2>
                <button className="btn btn-secondary" onClick={() => navigate(-1)}>
                    Back to Projects
                </button>
            </div>

            <div className="card shadow mb-4">
                <div className="card-body">
                    <div className="row">
                        <div className="col-md-3"><strong>Client:</strong> {details.clientName}</div>
                        <div className="col-md-3"><strong>Project:</strong> {details.projectName}</div>
                        <div className="col-md-3"><strong>Date:</strong> {details.projectDate}</div>
                        <div className="col-md-3"><strong>Version:</strong> {details.versionNumber}</div>
                    </div>
                </div>
            </div>

            <div className="card shadow">
                <div className="card-header">
                    <h4 className="mb-0">Modules</h4>
                </div>
                <div className="card-body">
                    <table className="table table-striped table-hover table-bordered mb-0">
                        <thead className="table-dark">
                            <tr>
                                <th>Room Name</th>
                                <th>Woodwork</th>
                                <th>Accessories</th>
                                <th>Miscellaneous Items</th>
                                <th>Total</th>
                                <th>Orders</th>
                            </tr>
                        </thead>
                        <tbody>
                            {details.modules.length === 0 ? (
                                <tr>
                                    <td colSpan={6} className="text-center">
                                        No Modules Found
                                    </td>
                                </tr>
                            ) : (
                                details.modules.map((module) => (
                                    <tr key={module.projectDetailId}>
                                        <td>{module.roomName}</td>
                                        <td>{module.woodwork.toFixed(2)}</td>
                                        <td>{module.accessories.toFixed(2)}</td>
                                        <td>{module.services.toFixed(2)}</td>
                                        <td>{module.total.toFixed(2)}</td>
                                        <td>
                                            <button
                                                className="btn btn-primary btn-sm"
                                                onClick={() => handleViewOrders(module.projectDetailId)}
                                            >
                                                View Orders
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {orders && (
                <div className="modal fade show d-block" tabIndex={-1} role="dialog">
                    <div className="modal-dialog modal-lg modal-dialog-centered" role="document">
                        <div className="modal-content">
                            <div className="modal-header">
                                <h5 className="modal-title">Orders: {orders.roomName}</h5>
                                <button
                                    type="button"
                                    className="btn-close"
                                    aria-label="Close"
                                    onClick={() => setOrders(null)}
                                />
                            </div>
                            <div className="modal-body">
                                {orders.orders.length === 0 ? (
                                    <p className="mb-0">No orders found.</p>
                                ) : (
                                    orders.orders.map((order) => (
                                        <div key={order.orderId} className="border-bottom mb-3 pb-3">
                                            <p><strong>Unit:</strong> {order.parent || "N/A"}</p>
                                            <p><strong>Materials:</strong> {order.materials || "N/A"}</p>
                                            <p><strong>Dimensions:</strong> {order.width || "N/A"} x {order.height || "N/A"} x {order.depth || "N/A"}</p>
                                            <p><strong>Accessories:</strong> {order.accessories || "N/A"}</p>
                                            <p><strong>Additional Items:</strong> {order.additionalItemName || "N/A"}</p>
                                            <p className="mb-0"><strong>Total:</strong> ₹{order.totalPrice.toFixed(2)}</p>
                                        </div>
                                    ))
                                )}
                            </div>
                            <div className="modal-footer">
                                <button className="btn btn-secondary" onClick={() => setOrders(null)}>
                                    Close
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {orders && <div className="modal-backdrop fade show" />}

        </div>

    );
}

export default ProjectDetail;
