import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import authService from "../services/authService";
export default function Login() { const navigate = useNavigate(); const [email,setEmail]=useState(""); 
const [password,setPassword]=useState(""); const [busy,setBusy]=useState(false); 
const submit=async(e:FormEvent)=>{e.preventDefault();setBusy(true);
try{await authService.login(email,password);toast.success("Login successful");
    navigate("/",{replace:true});}catch(error){authService.logout();
        const status=(error as { response?: { status?: number } }).response?.status;toast.error(status===500?"Unable to login. Please try again.":"Invalid username or password.");}
        finally{setBusy(false);}}; return <main className="login-page"><form className="login-card" onSubmit={submit}>
            <div className="login-brand">Walking Patterns<span>Interior Solutions</span></div>
            <h1>Welcome Back</h1>
            <p className="login-subtitle">Sign in to continue</p>
            <label className="form-label">User Name<input className="form-control" type="text" autoComplete="username" required value={email} onChange={e=>setEmail(e.target.value)}/></label>
            <label className="form-label">Password<input className="form-control" type="password" autoComplete="current-password" required value={password} onChange={e=>setPassword(e.target.value)}/>
            </label><button className="btn btn-primary w-100" disabled={busy}>{busy?"Signing in...":"Login"}</button></form></main>; }
