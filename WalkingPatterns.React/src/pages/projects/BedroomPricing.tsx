import { useEffect, useMemo, useState } from "react";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify";
import bedroomService, { type BedroomItemRequest, type BedroomPricing } from "../../services/bedroomService";
import ProjectCart from "../../components/ProjectCart";

function BedroomPricingPage() {
    const navigate = useNavigate();
    const parsedProjectId = Number(useParams().projectId);
    const [pricing, setPricing] = useState<BedroomPricing>();
    const [cartRefreshKey, setCartRefreshKey] = useState(0);
    const { register, control, handleSubmit, reset, formState: { errors } } = useForm<BedroomItemRequest>({
        defaultValues: { parent: "", utilityName: "", width: "", height: "", depth: "", coreMaterial: "", shutterMaterial: "", additionalItems: [], utilityNameOld: "Bedroom" }
    });
    const { fields, append, remove } = useFieldArray({ control, name: "additionalItems" });
    const parent = useWatch({ control, name: "parent" });
    const core = useWatch({ control, name: "coreMaterial" });
    const coreOptions = useMemo(() => pricing?.pricingData[parent] || {}, [pricing, parent]);
    const shutterOptions = useMemo(() => coreOptions[core] || {}, [coreOptions, core]);

    useEffect(() => { void bedroomService.getPricing().then(setPricing).catch((error) => { console.error(error); toast.error("Unable to load bedroom pricing"); }); }, []);

    const onSubmit = async (data: BedroomItemRequest) => {
        if (!Number.isInteger(parsedProjectId) || parsedProjectId <= 0) { toast.error("Invalid project"); return; }
        try {
            const result = await bedroomService.calculateAndSave(parsedProjectId, { ...data, utilityNameOld: "Bedroom" });
            toast.success(`Bedroom item saved. Total: ₹${result.totalPrice.toFixed(2)}`);
            setCartRefreshKey((key) => key + 1);
            reset({ parent: "", utilityName: "", width: "", height: "", depth: "", coreMaterial: "", shutterMaterial: "", additionalItems: [], utilityNameOld: "Bedroom" });
        } catch (error) {
            console.error(error);
            const responseError = error as { response?: { data?: { message?: string } } };
            toast.error(responseError.response?.data?.message || "Unable to save bedroom item");
        }
    };

    if (!Number.isInteger(parsedProjectId) || parsedProjectId <= 0) return <div className="container mt-5"><div className="alert alert-danger">Invalid project.</div></div>;
    return <div className="container py-3">
        <div className="pricing-page-header"><h2 className="page-title">Bedroom Pricing</h2><div className="pricing-page-actions"><button className="btn btn-secondary btn-sm" onClick={() => navigate(`/projects/${parsedProjectId}`)}>Back to Project</button><ProjectCart projectId={parsedProjectId} refreshKey={cartRefreshKey} sourceFilter="Bedroom" /></div></div>
        <form className="pricing-form" onSubmit={handleSubmit(onSubmit)} noValidate>
            <div className="card shadow-sm mb-3"><div className="card-body"><div className="row g-3">
                <div className="col-md-6"><label className="form-label">Parent / Unit Type</label><select className="form-select" {...register("parent", { required: "Parent is required" })}><option value="">Select unit type</option>{Object.keys(pricing?.pricingData || {}).map((item) => <option key={item} value={item}>{item}</option>)}</select>{errors.parent && <div className="text-danger">{errors.parent.message}</div>}</div>
                <div className="col-md-6"><label className="form-label">Utility / Room Name</label><input className="form-control" {...register("utilityName", { required: "Utility name is required" })} />{errors.utilityName && <div className="text-danger">{errors.utilityName.message}</div>}</div>
                {(["width", "height", "depth"] as const).map((field) => <div className="col-md-4" key={field}><label className="form-label">{field[0].toUpperCase() + field.slice(1)} (mm)</label><input type="number" min="0.01" step="any" className="form-control" {...register(field, { required: `${field} is required`, validate: (value) => Number(value) > 0 || `${field} must be greater than 0` })} />{errors[field] && <div className="text-danger">{errors[field]?.message}</div>}</div>)}
                <div className="col-md-6"><label className="form-label">Core Material</label><select className="form-select" {...register("coreMaterial", { required: "Core material is required" })}><option value="">Select core material</option>{Object.keys(coreOptions).map((item) => <option key={item} value={item}>{item}</option>)}</select>{errors.coreMaterial && <div className="text-danger">{errors.coreMaterial.message}</div>}</div>
                <div className="col-md-6"><label className="form-label">Shutter Material</label><select className="form-select" {...register("shutterMaterial", { required: "Shutter material is required" })}><option value="">Select shutter material</option>{Object.keys(shutterOptions).map((item) => <option key={item} value={item}>{item}</option>)}</select>{errors.shutterMaterial && <div className="text-danger">{errors.shutterMaterial.message}</div>}</div>
            </div></div></div>
            <div className="card shadow-sm mb-3"><div className="card-header d-flex justify-content-between align-items-center"><span>Additional Items</span><button type="button" className="btn btn-outline-primary btn-sm" onClick={() => append({ name: "", amount: 0, quantity: 1 })}>Add Item</button></div><div className="card-body">{fields.map((field, index) => <div className="pricing-additional-row" key={field.id}><div className="pricing-additional-name"><input className="form-control" placeholder="Name" {...register(`additionalItems.${index}.name`)} /></div><div className="pricing-additional-amount"><input type="number" min="0" step="any" className="form-control" placeholder="Amount" {...register(`additionalItems.${index}.amount`, { valueAsNumber: true, min: 0 })} /></div><div className="pricing-additional-quantity"><input type="number" min="1" step="1" className="form-control" placeholder="Quantity" {...register(`additionalItems.${index}.quantity`, { valueAsNumber: true, min: 1, required: true })} /></div><button type="button" className="btn btn-outline-danger btn-sm" onClick={() => remove(index)}>Remove</button></div>)}</div></div>
            <div className="pricing-submit"><button type="submit" className="btn btn-primary">Calculate & Save Bedroom Item</button></div>
        </form>
    </div>;
}
export default BedroomPricingPage;
