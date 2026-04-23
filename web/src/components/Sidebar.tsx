import { Link, useLocation } from 'react-router-dom'
import { 
  LayoutDashboard, 
  School, 
  Users, 
  UserSquare2, 
  DoorOpen, 
  Receipt,
  Menu,
  ChevronLeft,
  ShieldCheck,
  Calendar,
  BookOpen
} from 'lucide-react'
import { useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'

const menuItems = [
  { path: '/', icon: LayoutDashboard, label: 'Panel Control' },
  { path: '/schools', icon: School, label: 'Colegios' },
  { path: '/teachers', icon: UserSquare2, label: 'Profesores' },
  { path: '/students', icon: Users, label: 'Alumnos' },
  { path: '/classrooms', icon: DoorOpen, label: 'Aulas' },
  { path: '/schedules', icon: Calendar, label: 'Horarios' },
  { path: '/classunits', icon: BookOpen, label: 'Unidades de Clase' },
  { path: '/invoices', icon: Receipt, label: 'Facturas' },
  { path: '/administration', icon: ShieldCheck, label: 'Administración' },
]

export default function Sidebar() {
  const [isCollapsed, setIsCollapsed] = useState(false)
  const location = useLocation()

  return (
    <motion.aside 
      initial={false}
      animate={{ width: isCollapsed ? 80 : 280 }}
      className="bg-white border-r border-surface-200 h-screen sticky top-0 flex flex-col z-50 overflow-hidden shadow-sm"
    >
      {/* Logo Section */}
      <div className="p-6 flex items-center justify-between border-b border-surface-100 h-20">
        <AnimatePresence mode="wait">
          {!isCollapsed && (
            <motion.div
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              className="flex items-center gap-3 overflow-hidden whitespace-nowrap"
            >
              <div className="w-8 h-8 bg-brand-600 rounded-lg flex items-center justify-center shrink-0">
                <School className="text-white w-5 h-5" />
              </div>
              <span className="font-display font-bold text-xl tracking-tight text-brand-900">Colegio ERP</span>
            </motion.div>
          )}
        </AnimatePresence>
        
        <button 
          onClick={() => setIsCollapsed(!isCollapsed)}
          className="p-2 hover:bg-surface-50 rounded-lg transition-colors text-surface-500"
        >
          {isCollapsed ? <Menu size={20} /> : <ChevronLeft size={20} />}
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
        {menuItems.map((item) => {
          const isActive = location.pathname === item.path
          return (
            <Link key={item.path} to={item.path}>
              <motion.div
                whileHover={{ x: 4 }}
                whileTap={{ scale: 0.98 }}
                className={`flex items-center gap-4 px-4 py-3 rounded-xl transition-all duration-200 group ${
                  isActive 
                    ? 'bg-brand-50 text-brand-600 font-semibold' 
                    : 'text-surface-500 hover:bg-surface-50 hover:text-surface-900'
                }`}
              >
                <item.icon size={22} className={`${isActive ? 'text-brand-600' : 'group-hover:text-surface-900'} shrink-0`} />
                <AnimatePresence mode="wait">
                  {!isCollapsed && (
                    <motion.span
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      exit={{ opacity: 0 }}
                      className="whitespace-nowrap"
                    >
                      {item.label}
                    </motion.span>
                  )}
                </AnimatePresence>
              </motion.div>
            </Link>
          )
        })}
      </nav>

      {/* Footer / Account */}
      <div className="p-4 border-top border-surface-100">
        <div className={`p-3 rounded-xl hover:bg-surface-50 transition-colors flex items-center gap-3 cursor-pointer ${isCollapsed ? 'justify-center' : ''}`}>
           <div className="w-10 h-10 rounded-full bg-surface-200 shrink-0 overflow-hidden">
             <img src={`https://ui-avatars.com/api/?name=Admin&background=random`} alt="Admin" />
           </div>
           {!isCollapsed && (
             <div className="flex flex-col overflow-hidden">
               <span className="text-sm font-semibold text-surface-900 truncate">Administrador</span>
               <span className="text-xs text-surface-500 truncate">admin@colegio.edu</span>
             </div>
           )}
        </div>
      </div>
    </motion.aside>
  )
}
