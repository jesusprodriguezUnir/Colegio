import { Bell, Search, UserCircle } from 'lucide-react'
import { useLocation } from 'react-router-dom'

export default function Header() {
  const location = useLocation()
  
  const getPageTitle = () => {
    switch (location.pathname) {
      case '/': return 'Panel de Control'
      case '/schools': return 'Gestión de Colegios'
      case '/teachers': return 'Personal Docente'
      case '/students': return 'Base de Alumnos'
      case '/classrooms': return 'Gestión de Aulas'
      case '/invoices': return 'Facturación y Pagos'
      default: return 'Colegio ERP'
    }
  }

  return (
    <header className="h-20 bg-white/70 backdrop-blur-lg border-b border-surface-200 sticky top-0 z-40 px-8 flex items-center justify-between">
      <div className="flex flex-col">
        <h1 className="text-xl font-bold font-display text-surface-900">{getPageTitle()}</h1>
        <div className="flex items-center gap-2 text-xs text-surface-500">
          <span>Colegio ERP</span>
          <span>/</span>
          <span className="capitalize">{location.pathname.replace('/', '') || 'Dashboard'}</span>
        </div>
      </div>

      <div className="flex items-center gap-6">
        {/* Search Bar */}
        <div className="hidden md:flex items-center bg-surface-100 rounded-xl px-4 py-2 border border-surface-200 focus-within:border-brand-500 focus-within:ring-4 focus-within:ring-brand-500/10 transition-all w-64">
          <Search size={18} className="text-surface-400" />
          <input 
            type="text" 
            placeholder="Buscar..." 
            className="bg-transparent border-none outline-none text-sm ml-2 w-full text-surface-900 placeholder:text-surface-400"
          />
        </div>

        {/* Actions */}
        <div className="flex items-center gap-3">
          <button className="p-2.5 hover:bg-surface-100 rounded-xl transition-colors relative text-surface-600">
            <Bell size={20} />
            <span className="absolute top-2 right-2 w-2 h-2 bg-red-500 border-2 border-white rounded-full"></span>
          </button>
          
          <div className="h-6 w-px bg-surface-200 mx-1"></div>
          
          <button className="flex items-center gap-2 p-1.5 hover:bg-surface-100 rounded-xl transition-colors">
            <UserCircle size={24} className="text-surface-400" />
            <span className="text-sm font-medium text-surface-700 hidden sm:inline">Admin</span>
          </button>
        </div>
      </div>
    </header>
  )
}
