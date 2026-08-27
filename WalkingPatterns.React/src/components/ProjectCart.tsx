import { useCallback, useEffect, useState } from "react";
import { toast } from "react-toastify";
import type { ProjectCartItem, ProjectCartSource } from "../models/Project";
import projectService from "../services/projectService";

interface ProjectCartProps {
    projectId: number;
    refreshKey?: number;
    sourceFilter?: ProjectCartSource;
    onRefreshReady?: (refresh: () => Promise<void>) => void;
}

function ProjectCart({ projectId, refreshKey = 0, sourceFilter, onRefreshReady }: ProjectCartProps) {
    const [allItems, setAllItems] = useState<ProjectCartItem[]>([]);
    const [open, setOpen] = useState(false);
    const [checkoutResult, setCheckoutResult] = useState<Awaited<ReturnType<typeof projectService.checkoutProject>> | null>(null);

    const refresh = useCallback(async () => {
        try {
            setAllItems(await projectService.getProjectCart(projectId));
        } catch (error) {
            console.error(error);
            toast.error("Unable to load cart");
        }
    }, [projectId]);

    useEffect(() => {
        onRefreshReady?.(refresh);
    }, [onRefreshReady, refresh]);

    useEffect(() => {
        let active = true;
        void projectService.getProjectCart(projectId)
            .then((data) => {
                if (active) setAllItems(data);
            })
            .catch((error) => {
                console.error(error);
                if (active) toast.error("Unable to load cart");
            });
        return () => {
            active = false;
        };
    }, [projectId, refreshKey]);

    const items = sourceFilter
        ? allItems.filter((item) => item.source === sourceFilter)
        : allItems;

    const deleteItem = async (item: ProjectCartItem) => {
        if (!window.confirm("Are you sure you want to remove this cart item?")) return;
        try {
            await projectService.deleteProjectCartItem(projectId, item.source, item.id);
            await refresh();
            toast.success("Cart item deleted successfully");
        } catch (error) {
            console.error(error);
            toast.error("Unable to delete cart item");
        }
    };

    const checkout = async () => {
        if (items.length === 0 || !window.confirm("Are you sure you want to checkout this cart?")) return;
        try {
            const result = await projectService.checkoutProject(projectId);
            setCheckoutResult(result);
            await refresh();
            toast.success("Project checkout completed successfully");
        } catch (error) {
            console.error(error);
            toast.error("Unable to checkout project cart");
        }
    };

    return <>
        <button type="button" className="btn btn-outline-primary btn-sm" onClick={() => setOpen(true)}>
            {sourceFilter ? `${sourceFilter === "OtherWoodwork" ? "Other Woodwork" : sourceFilter} Cart` : "Cart"} <span className="badge text-bg-primary ms-1">{items.length}</span>
        </button>
        {open && <>
            <div className="modal fade show d-block" tabIndex={-1} role="dialog">
                <div className="modal-dialog modal-xl modal-dialog-centered" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title">{sourceFilter ? `${sourceFilter === "OtherWoodwork" ? "Other Woodwork" : sourceFilter} Cart` : "Cart"} ({items.length})</h5>
                            <button type="button" className="btn-close" aria-label="Close" onClick={() => setOpen(false)} />
                        </div>
                        <div className="modal-body">
                            <div className="d-flex justify-content-end gap-2 mb-3">
                                <button type="button" className="btn btn-outline-primary btn-sm" onClick={() => void refresh()}>Refresh</button>
                                {!sourceFilter && <button type="button" className="btn btn-success btn-sm" disabled={items.length === 0} onClick={() => void checkout()}>Checkout</button>}
                            </div>
                            {items.length === 0 ? <p className="mb-0">No cart items found.</p> : <div className="table-responsive">
                                <table className="table table-striped table-hover table-bordered mb-0">
                                    <thead className="table-light"><tr><th>Source</th><th>Room/Utility</th><th>Material</th><th>Dimensions</th><th>Total</th><th>Action</th></tr></thead>
                                    <tbody>{items.map((item) => <tr key={`${item.source}-${item.id}`}>
                                        <td>{item.source}</td><td>{item.utilityName || item.utilityNameOld || "N/A"}</td><td>{item.materials || "N/A"}</td>
                                        <td>{item.width || "N/A"} x {item.height || "N/A"} x {item.depth || "N/A"}</td><td>₹{item.totalPrice.toFixed(2)}</td>
                                        <td><button type="button" className="btn btn-danger btn-sm" onClick={() => void deleteItem(item)}>Delete</button></td>
                                    </tr>)}</tbody>
                                </table>
                            </div>}
                            {checkoutResult && <div className="alert alert-success mt-3 mb-0">
                                <strong>Checkout complete:</strong> {checkoutResult.checkedOutItemCount} item(s), cart total ₹{checkoutResult.cartTotal.toFixed(2)}, grand total ₹{checkoutResult.grandTotal.toFixed(2)}, discount ₹{checkoutResult.discountAmount.toFixed(2)}, discounted total ₹{checkoutResult.discountedTotal.toFixed(2)}, version {checkoutResult.versionNumber}.
                            </div>}
                        </div>
                    </div>
                </div>
            </div>
            <div className="modal-backdrop fade show" />
        </>}
    </>;
}

export default ProjectCart;
