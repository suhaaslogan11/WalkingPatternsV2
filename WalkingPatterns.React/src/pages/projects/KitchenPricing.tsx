import { useEffect, useMemo, useState } from "react";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify";
import kitchenService, {
    type KitchenItemRequest,
    type KitchenPricing
} from "../../services/kitchenService";

type FormValues = KitchenItemRequest;

function KitchenPricingPage() {
    const navigate = useNavigate();
    const { projectId } = useParams();
    const parsedProjectId = Number(projectId);
    const [pricing, setPricing] = useState<KitchenPricing>();
    const { register, control, handleSubmit, formState: { errors }, setValue } = useForm<FormValues>({
        defaultValues: {
            parent: "", utilityName: "", width: "", height: "", depth: "", materials: "",
            accessories: [], quantities: [], additionalItems: [], utilityNameOld: "KitchenUtility"
        }
    });
    const { fields, append, remove } = useFieldArray({ control, name: "additionalItems" });
    const selectedParent = useWatch({ control, name: "parent" });
    const selectedAccessories = useWatch({ control, name: "accessories" }) || [];
    const selectedQuantities = useWatch({ control, name: "quantities" }) || [];
    const accessoryOptions = useMemo(
        () => pricing?.parentOptions[selectedParent] || [],
        [pricing, selectedParent]
    );

    useEffect(() => {
        void kitchenService.getPricing()
            .then(setPricing)
            .catch((error) => {
                console.error(error);
                toast.error("Unable to load kitchen pricing");
            });
    }, []);

    const onParentChange = (value: string) => {
        setValue("parent", value);
        setValue("accessories", []);
        setValue("quantities", []);
    };

    const onSubmit = async (data: FormValues) => {
        if (!Number.isInteger(parsedProjectId) || parsedProjectId <= 0) {
            toast.error("Invalid project");
            return;
        }
        try {
            const result = await kitchenService.calculateAndSave(parsedProjectId, {
                ...data,
                utilityNameOld: "KitchenUtility",
                accessories: data.accessories || [],
                quantities: data.quantities || []
            });
            toast.success(`Kitchen item saved. Total: ₹${result.totalPrice.toFixed(2)}`);
        } catch (error) {
            console.error(error);
            const responseError = error as { response?: { data?: { message?: string } } };
            toast.error(responseError.response?.data?.message || "Unable to save kitchen item");
        }
    };

    if (!Number.isInteger(parsedProjectId) || parsedProjectId <= 0)
        return <div className="container mt-5"><div className="alert alert-danger">Invalid project.</div></div>;

    return (
        <div className="container mt-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2>Kitchen Pricing</h2>
                <button className="btn btn-secondary" onClick={() => navigate(`/projects/${parsedProjectId}`)}>Back to Project</button>
            </div>
            <form onSubmit={handleSubmit(onSubmit)} noValidate>
                <div className="card shadow mb-4"><div className="card-body">
                    <div className="row g-3">
                        <div className="col-md-6"><label className="form-label">Parent / Unit Type</label>
                            <select className="form-select" {...register("parent", { required: "Parent is required" })} onChange={(e) => onParentChange(e.target.value)}>
                                <option value="">Select unit type</option>{Object.keys(pricing?.parentOptions || {}).map((parent) => <option key={parent} value={parent}>{parent}</option>)}
                            </select>{errors.parent && <div className="text-danger">{errors.parent.message}</div>}
                        </div>
                        <div className="col-md-6"><label className="form-label">Utility / Room Name</label><input className="form-control" {...register("utilityName", { required: "Utility name is required" })} />{errors.utilityName && <div className="text-danger">{errors.utilityName.message}</div>}</div>
                        {(["width", "height", "depth"] as const).map((field) => <div className="col-md-4" key={field}><label className="form-label">{field[0].toUpperCase() + field.slice(1)} (mm)</label><input type="number" min="0.01" step="any" className="form-control" {...register(field, { required: `${field} is required`, validate: (value) => Number(value) > 0 || `${field} must be greater than 0` })} />{errors[field] && <div className="text-danger">{errors[field]?.message}</div>}</div>)}
                        <div className="col-md-6"><label className="form-label">Shutter Material</label><select className="form-select" {...register("materials", { required: "Material is required" })}><option value="">Select material</option>{Object.keys(pricing?.materials || {}).map((material) => <option key={material} value={material}>{material}</option>)}</select>{errors.materials && <div className="text-danger">{errors.materials.message}</div>}</div>
                    </div>
                </div></div>
                <div className="card shadow mb-4"><div className="card-header">Accessories</div><div className="card-body">{accessoryOptions.length === 0 ? <p className="mb-0">Select a parent to view accessories.</p> : accessoryOptions.map((option) => { const checked = selectedAccessories.includes(option.name); const selectedIndex = selectedAccessories.indexOf(option.name); return <div className="row align-items-center mb-2" key={option.name}><div className="col-md-7"><label><input type="checkbox" className="form-check-input me-2" checked={checked} onChange={(e) => { const next = e.target.checked ? [...selectedAccessories, option.name] : selectedAccessories.filter((name) => name !== option.name); const quantities = e.target.checked ? [...selectedQuantities, "1"] : selectedQuantities.filter((_, index) => index !== selectedIndex); setValue("accessories", next); setValue("quantities", quantities); }} />{option.name}</label></div>{checked && <div className="col-md-3"><input type="number" min="1" step="1" className="form-control" {...register(`quantities.${selectedIndex}`, { required: true, min: 1, pattern: /^\d+$/ })} placeholder="Quantity" /></div>}{errors.quantities?.[selectedIndex] && <div className="text-danger">Quantity must be a positive integer.</div>}</div>; })}</div></div>
                <div className="card shadow mb-4"><div className="card-header d-flex justify-content-between"><span>Additional Items</span><button type="button" className="btn btn-outline-primary btn-sm" onClick={() => append({ name: "", amount: 0, quantity: 1 })}>Add Item</button></div><div className="card-body">{fields.map((field, index) => <div className="row g-2 mb-2" key={field.id}><div className="col-md-5"><input className="form-control" placeholder="Name" {...register(`additionalItems.${index}.name`)} /></div><div className="col-md-3"><input type="number" min="0" step="any" className="form-control" placeholder="Amount" {...register(`additionalItems.${index}.amount`, { valueAsNumber: true, min: 0 })} /></div><div className="col-md-3"><input type="number" min="1" step="1" className="form-control" placeholder="Quantity" {...register(`additionalItems.${index}.quantity`, { valueAsNumber: true, min: 1, required: true })} /></div><div className="col-md-1"><button type="button" className="btn btn-outline-danger" onClick={() => remove(index)}>×</button></div></div>)}</div></div>
                <button type="submit" className="btn btn-primary">Calculate & Save Kitchen Item</button>
            </form>
        </div>
    );
}

export default KitchenPricingPage;
