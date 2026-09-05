import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { useAuth } from "../auth/authContext";
export default function Login() { const navigate = useNavigate(); const { login, logout } = useAuth(); const [email,setEmail]=useState("");
const [password,setPassword]=useState(""); const [busy,setBusy]=useState(false); 
const submit=async(e:FormEvent)=>{e.preventDefault();setBusy(true);
try{await login(email,password);toast.success("Login successful");
    navigate("/",{replace:true});}catch(error){logout();
        const status=(error as { response?: { status?: number } }).response?.status;toast.error(status===500?"Unable to login. Please try again.":"Invalid username or password.");}
        finally{setBusy(false);}}; return <main className="login-page"><form className="login-card" onSubmit={submit}>
            <div className="login-brand">WalkingPatterns<span>Architecture & Interior Studio</span></div>
            <label className="form-label"><input className="form-control" type="text" autoComplete="username" placeholder="Username" aria-label="Username" required value={email} onChange={e=>setEmail(e.target.value)}/></label>
            <label className="form-label"><input className="form-control" type="password" autoComplete="current-password" placeholder="Password" aria-label="Password" required value={password} onChange={e=>setPassword(e.target.value)}/>
            </label><button className="btn btn-primary w-100" disabled={busy}>{busy?"Signing in...":"Login"}</button></form></main>; }
