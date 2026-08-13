import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify";
import type {
    ProjectDetailPage,
    ProjectFinancials,
    ProjectOrders,
    ProjectCartItem,
    ProjectCheckoutResponse
} from "../../models/Project";
import projectService from "../../services/projectService";

function ProjectDetail() {

    type DiscountForm = {
        discountAmount: number;
    };

    const navigate = useNavigate();
    const { projectId } = useParams();
    const parsedProjectId = Number(projectId);
    const [details, setDetails] = useState<ProjectDetailPage>();
    const [financials, setFinancials] = useState<ProjectFinancials>();
    const [orders, setOrders] = useState<ProjectOrders | null>(null);
    const [cartItems, setCartItems] = useState<ProjectCartItem[]>([]);
    const [checkoutResult, setCheckoutResult] = useState<ProjectCheckoutResponse | null>(null);
    const [renamingModuleId, setRenamingModuleId] = useState<number | null>(null);
    const [newRoomName, setNewRoomName] = useState("");
    const {
        register,
        handleSubmit,
        reset,
        formState: { errors }
    } = useForm<DiscountForm>({
        defaultValues: { discountAmount: 0 }
    });

    useEffect(() => {

        if (!Number.isInteger(parsedProjectId) || parsedProjectId <= 0)
            return;

        void Promise.all([
            projectService.getProjectDetails(parsedProjectId),
            projectService.getProjectFinancials(parsedProjectId),
            projectService.getProjectCart(parsedProjectId)
        ])
            .then(([projectDetails, projectFinancials, projectCart]) => {
                setDetails(projectDetails);
                setFinancials(projectFinancials);
                setCartItems(projectCart);
                reset({ discountAmount: projectFinancials.discountAmount });
            })
            .catch((error) => {
                console.error(error);
                toast.error("Unable to load project details");
            });

    }, [parsedProjectId, reset]);

    const onApplyDiscount = async (data: DiscountForm) => {

        try {

            const updatedFinancials = await projectService.applyDiscount(
                parsedProjectId,
                data.discountAmount
            );

            setFinancials(updatedFinancials);
            reset({ discountAmount: updatedFinancials.discountAmount });
            toast.success("Discount applied successfully");

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to apply discount");

        }

    };

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

    const handleDeleteOrder = async (orderId: number) => {

        if (!orders || !window.confirm("Are you sure you want to delete this order?"))
            return;

        try {

            await projectService.deleteOrder(orderId);

            const [updatedOrders, updatedFinancials] = await Promise.all([
                projectService.getProjectDetailOrders(orders.projectDetailId),
                projectService.getProjectFinancials(parsedProjectId)
            ]);

            setOrders(updatedOrders);
            setFinancials(updatedFinancials);
            toast.success("Order deleted successfully");

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to delete order");

        }

    };

    const handleDeleteModule = async (projectDetailId: number, roomName: string) => {

        if (!window.confirm(`Are you sure you want to delete the ${roomName} module?`))
            return;

        try {

            await projectService.deleteProjectModule(parsedProjectId, projectDetailId);

            const [updatedDetails, updatedFinancials] = await Promise.all([
                projectService.getProjectDetails(parsedProjectId),
                projectService.getProjectFinancials(parsedProjectId)
            ]);

            setDetails(updatedDetails);
            setFinancials(updatedFinancials);
            setOrders(null);
            toast.success("Module deleted successfully");

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to delete module");

        }

    };

    const handleRenameModule = async (projectDetailId: number) => {
        if (!newRoomName.trim()) {
            toast.error("Room name is required");
            return;
        }

        try {
            await projectService.renameProjectModule(
                parsedProjectId,
                projectDetailId,
                newRoomName.trim()
            );

            const updatedDetails = await projectService.getProjectDetails(parsedProjectId);
            setDetails(updatedDetails);

            if (orders?.projectDetailId === projectDetailId) {
                setOrders(await projectService.getProjectDetailOrders(projectDetailId));
            }

            setRenamingModuleId(null);
            setNewRoomName("");
            toast.success("Module renamed successfully");
        }
        catch (error) {
            console.error(error);
            toast.error("Unable to rename module");
        }
    };

    const handleDeleteCartItem = async (item: ProjectCartItem) => {
        if (!window.confirm("Are you sure you want to remove this cart item?"))
            return;

        try {
            await projectService.deleteProjectCartItem(parsedProjectId, item.source, item.id);
            const updatedCart = await projectService.getProjectCart(parsedProjectId);
            setCartItems(updatedCart);
            toast.success("Cart item deleted successfully");
        }
        catch (error) {
            console.error(error);
            toast.error("Unable to delete cart item");
        }
    };

    const handleCheckout = async () => {
        if (cartItems.length === 0 || !window.confirm("Are you sure you want to checkout this cart?"))
            return;

        try {
            const result = await projectService.checkoutProject(parsedProjectId);
            const [updatedDetails, updatedFinancials, updatedCart] = await Promise.all([
                projectService.getProjectDetails(parsedProjectId),
                projectService.getProjectFinancials(parsedProjectId),
                projectService.getProjectCart(parsedProjectId)
            ]);

            setDetails(updatedDetails);
            setFinancials(updatedFinancials);
            setCartItems(updatedCart);
            setCheckoutResult(result);
            toast.success("Project checkout completed successfully");
        }
        catch (error) {
            console.error(error);
            toast.error("Unable to checkout project cart");
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

    if (!details || !financials) {
        return <div className="container mt-5">Loading project details...</div>;
    }

    return (

        <div className="container mt-5">

            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2 className="mb-0">Project Details</h2>
                <div><button className="btn btn-success me-2" onClick={() => navigate(`/projects/${parsedProjectId}/kitchen`)}>Add Kitchen</button><button className="btn btn-success me-2" onClick={() => navigate(`/projects/${parsedProjectId}/bedroom`)}>Add Bedroom</button><button className="btn btn-success me-2" onClick={() => navigate(`/projects/${parsedProjectId}/other-woodwork`)}>Add Other Woodwork</button><button className="btn btn-success me-2" onClick={() => navigate(`/projects/${parsedProjectId}/hds`)}>Add HDS</button><button className="btn btn-secondary" onClick={() => navigate(-1)}>Back to Projects</button></div>
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

            <div className="card shadow mb-4">
                <div className="card-header">
                    <h4 className="mb-0">Price Summary</h4>
                </div>
                <div className="card-body">
                    <div className="row mb-3">
                        <div className="col-md-4"><strong>Grand Total:</strong> ₹{financials.grandTotal.toFixed(2)}</div>
                        <div className="col-md-4"><strong>Discount:</strong> ₹{financials.discountAmount.toFixed(2)}</div>
                        <div className="col-md-4"><strong>Discounted Total:</strong> ₹{financials.discountedTotal.toFixed(2)}</div>
                    </div>
                    <form onSubmit={handleSubmit(onApplyDiscount)} noValidate>
                        <div className="row g-3 align-items-end">
                            <div className="col-md-4">
                                <label className="form-label">Discount Amount</label>
                                <input
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    className="form-control"
                                    {...register("discountAmount", {
                                        required: "Discount amount is required",
                                        min: {
                                            value: 0,
                                            message: "Discount cannot be negative"
                                        },
                                        valueAsNumber: true
                                    })}
                                />
                                {errors.discountAmount && (
                                    <div className="text-danger">
                                        {errors.discountAmount.message}
                                    </div>
                                )}
                            </div>
                            <div className="col-md-3">
                                <button type="submit" className="btn btn-primary">
                                    Apply Discount
                                </button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>

            <div className="card shadow mb-4">
                <div className="card-header d-flex justify-content-between align-items-center">
                    <h4 className="mb-0">Cart</h4>
                    <div>
                        <button
                            type="button"
                            className="btn btn-outline-primary btn-sm me-2"
                            onClick={() => void projectService.getProjectCart(parsedProjectId).then(setCartItems)}
                        >
                            Refresh
                        </button>
                        <button
                            type="button"
                            className="btn btn-success btn-sm"
                            disabled={cartItems.length === 0}
                            onClick={() => void handleCheckout()}
                        >
                            Checkout
                        </button>
                    </div>
                </div>
                <div className="card-body">
                    {cartItems.length === 0 ? (
                        <p className="mb-0">No cart items found.</p>
                    ) : (
                        <div className="table-responsive">
                            <table className="table table-striped table-bordered mb-0">
                                <thead className="table-light">
                                    <tr>
                                        <th>Source</th>
                                        <th>Room/Utility</th>
                                        <th>Material</th>
                                        <th>Dimensions</th>
                                        <th>Total</th>
                                        <th>Action</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {cartItems.map((item) => (
                                        <tr key={`${item.source}-${item.id}`}>
                                            <td>{item.source}</td>
                                            <td>{item.utilityName || item.utilityNameOld || "N/A"}</td>
                                            <td>{item.materials || "N/A"}</td>
                                            <td>{item.width || "N/A"} x {item.height || "N/A"} x {item.depth || "N/A"}</td>
                                            <td>₹{item.totalPrice.toFixed(2)}</td>
                                            <td>
                                                <button
                                                    type="button"
                                                    className="btn btn-danger btn-sm"
                                                    onClick={() => void handleDeleteCartItem(item)}
                                                >
                                                    Delete
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                    {checkoutResult && (
                        <div className="alert alert-success mt-3 mb-0">
                            <strong>Checkout complete:</strong>{" "}
                            {checkoutResult.checkedOutItemCount} item(s), cart total ₹{checkoutResult.cartTotal.toFixed(2)},{" "}
                            grand total ₹{checkoutResult.grandTotal.toFixed(2)}, discount ₹{checkoutResult.discountAmount.toFixed(2)},{" "}
                            discounted total ₹{checkoutResult.discountedTotal.toFixed(2)}, version {checkoutResult.versionNumber}.
                        </div>
                    )}
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
                                            {renamingModuleId === module.projectDetailId ? (
                                                <div className="input-group input-group-sm mb-2">
                                                    <input
                                                        className="form-control"
                                                        value={newRoomName}
                                                        onChange={(event) => setNewRoomName(event.target.value)}
                                                        placeholder="New room name"
                                                    />
                                                    <button
                                                        className="btn btn-success"
                                                        onClick={() => void handleRenameModule(module.projectDetailId)}
                                                    >
                                                        Save
                                                    </button>
                                                    <button
                                                        className="btn btn-secondary"
                                                        onClick={() => {
                                                            setRenamingModuleId(null);
                                                            setNewRoomName("");
                                                        }}
                                                    >
                                                        Cancel
                                                    </button>
                                                </div>
                                            ) : (
                                                <button
                                                    className="btn btn-outline-secondary btn-sm"
                                                    onClick={() => {
                                                        setRenamingModuleId(module.projectDetailId);
                                                        setNewRoomName(module.roomName);
                                                    }}
                                                >
                                                    Rename
                                                </button>
                                            )}
                                            <button
                                                className="btn btn-primary btn-sm"
                                                onClick={() => handleViewOrders(module.projectDetailId)}
                                            >
                                                View Orders
                                            </button>
                                            <button
                                                className="btn btn-danger btn-sm ms-2"
                                                onClick={() => handleDeleteModule(module.projectDetailId, module.roomName)}
                                            >
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
                                            <button
                                                type="button"
                                                className="btn btn-danger btn-sm mt-2"
                                                onClick={() => handleDeleteOrder(order.orderId)}
                                            >
                                                Delete
                                            </button>
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
