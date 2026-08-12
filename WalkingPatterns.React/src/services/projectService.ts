import api from "../api";
import type {
    AddProjectRequest,
    Project,
    ProjectDetailPage,
    ProjectFinancials,
    ProjectOrders,
    ProjectCartItem,
    ProjectCartSource,
    ProjectCheckoutResponse
} from "../models/Project";

const getProjects = async (clientId: number): Promise<Project[]> => {
    const response = await api.get(`/clients/${clientId}/projects`);
    return response.data;
};

const getProject = async (id: number): Promise<Project> => {
    const response = await api.get(`/projects/${id}`);
    return response.data;
};

const getProjectDetails = async (projectId: number): Promise<ProjectDetailPage> => {
    const response = await api.get(`/projects/${projectId}/details`);
    return response.data;
};

const getProjectDetailOrders = async (projectDetailId: number): Promise<ProjectOrders> => {
    const response = await api.get(`/project-details/${projectDetailId}/orders`);
    return response.data;
};

const getProjectFinancials = async (projectId: number): Promise<ProjectFinancials> => {
    const response = await api.get(`/projects/${projectId}/grand-total`);
    return response.data;
};

const applyDiscount = async (
    projectId: number,
    discountAmount: number
): Promise<ProjectFinancials> => {
    const response = await api.post(`/projects/${projectId}/discount`, {
        discountAmount
    });
    return response.data;
};

const deleteOrder = async (orderId: number) => {
    await api.delete(`/orders/${orderId}`);
};

const deleteProjectModule = async (projectId: number, projectDetailId: number) => {
    await api.delete(`/projects/${projectId}/modules/${projectDetailId}`);
};

const getProjectCart = async (projectId: number): Promise<ProjectCartItem[]> => {
    const response = await api.get(`/projects/${projectId}/cart`);
    return response.data;
};

const deleteProjectCartItem = async (
    projectId: number,
    source: ProjectCartSource,
    itemId: number
) => {
    await api.delete(`/projects/${projectId}/cart/${source}/${itemId}`);
};

const checkoutProject = async (projectId: number): Promise<ProjectCheckoutResponse> => {
    const response = await api.post(`/projects/${projectId}/checkout`);
    return response.data;
};

const addProject = async (
    clientId: number,
    project: AddProjectRequest
): Promise<Project> => {
    const response = await api.post(`/clients/${clientId}/projects`, project);
    return response.data;
};

const deleteProject = async (id: number) => {
    await api.delete(`/projects/${id}`);
};

const updateProject = async (
    id: number,
    project: AddProjectRequest
): Promise<Project> => {
    const response = await api.put(`/projects/${id}`, project);
    return response.data;
};

export default {
    getProjects,
    getProject,
    getProjectDetails,
    getProjectDetailOrders,
    getProjectFinancials,
    applyDiscount,
    deleteOrder,
    deleteProjectModule,
    getProjectCart,
    deleteProjectCartItem,
    checkoutProject,
    addProject,
    deleteProject,
    updateProject
};
