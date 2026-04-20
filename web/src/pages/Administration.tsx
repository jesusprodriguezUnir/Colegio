import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  ShieldCheck, 
  Database, 
  RotateCcw, 
  Trash2, 
  AlertTriangle,
  CheckCircle2,
  Users,
  School,
  UserSquare2,
  Calendar,
  Receipt,
  FileText,
  Clock
} from 'lucide-react'
import { maintenanceApi } from '../services/api'

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 }
  }
}

const itemVariants = {
  hidden: { y: 20, opacity: 0 },
  visible: { y: 0, opacity: 1 }
}

export default function Administration() {
  const [stats, setStats] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [actionLoading, setActionLoading] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null)

  const fetchStats = async () => {
    try {
      const res = await maintenanceApi.getStats()
      setStats(res.data)
    } catch (e) {
      console.error('Error fetching stats', e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchStats()
  }, [])

  const handleAction = async (action: () => Promise<any>, successMsg: string) => {
    if (!confirm('¿Estás seguro de realizar esta acción? Esta operación no se puede deshacer.')) return
    
    setActionLoading(true)
    setMessage(null)
    try {
      await action()
      setMessage({ type: 'success', text: successMsg })
      await fetchStats()
    } catch (e) {
      setMessage({ type: 'error', text: 'Error al realizar la operación' })
    } finally {
      setActionLoading(false)
      setTimeout(() => setMessage(null), 5000)
    }
  }

  const statItems = [
    { label: 'Colegios', value: stats?.schools ?? 0, icon: School, color: 'text-purple-600', bg: 'bg-purple-50' },
    { label: 'Profesores', value: stats?.teachers ?? 0, icon: UserSquare2, color: 'text-indigo-600', bg: 'bg-indigo-50' },
    { label: 'Alumnos', value: stats?.students ?? 0, icon: Users, color: 'text-blue-600', bg: 'bg-blue-50' },
    { label: 'Padres', value: stats?.parents ?? 0, icon: Users, color: 'text-emerald-600', bg: 'bg-emerald-50' },
    { label: 'Clases', value: stats?.classrooms ?? 0, icon: Database, color: 'text-amber-600', bg: 'bg-amber-50' },
    { label: 'Horarios', value: stats?.schedules ?? 0, icon: Calendar, color: 'text-orange-600', bg: 'bg-orange-50' },
    { label: 'Facturas', value: stats?.invoices ?? 0, icon: Receipt, color: 'text-rose-600', bg: 'bg-rose-50' },
  ]

  return (
    <motion.div 
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="space-y-8 max-w-6xl mx-auto"
    >
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <div className="w-8 h-8 rounded-lg bg-brand-100 flex items-center justify-center">
              <ShieldCheck className="text-brand-600 w-5 h-5" />
            </div>
            <h2 className="text-3xl font-bold text-surface-900 tracking-tight">Administración</h2>
          </div>
          <p className="text-surface-500">Mantenimiento y gestión de la base de datos.</p>
        </div>
      </div>

      <AnimatePresence>
        {message && (
          <motion.div
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            className={`p-4 rounded-xl flex items-center gap-3 ${
              message.type === 'success' ? 'bg-emerald-50 text-emerald-700 border border-emerald-100' : 'bg-rose-50 text-rose-700 border border-rose-100'
            }`}
          >
            {message.type === 'success' ? <CheckCircle2 size={20} /> : <AlertTriangle size={20} />}
            <span className="font-medium">{message.text}</span>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left Column: Stats */}
        <div className="lg:col-span-2 space-y-6">
          <motion.div variants={itemVariants} className="glass-card p-6">
            <h3 className="text-lg font-bold mb-6 flex items-center gap-2">
              <Database size={20} className="text-surface-400" />
              Estado de las Tablas
            </h3>
            
            {loading ? (
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {[...Array(6)].map((_, i) => (
                  <div key={i} className="h-24 bg-surface-100 rounded-xl animate-pulse" />
                ))}
              </div>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {statItems.map((item, idx) => (
                  <div key={idx} className="p-4 rounded-xl border border-surface-100 bg-surface-50/50 hover:bg-white hover:shadow-sm transition-all duration-300">
                    <div className="flex items-center gap-3 mb-2">
                      <div className={`p-2 rounded-lg ${item.bg}`}>
                        <item.icon className={`${item.color} w-4 h-4`} />
                      </div>
                      <span className="text-sm font-medium text-surface-500">{item.label}</span>
                    </div>
                    <div className="text-2xl font-bold text-surface-900">{item.value}</div>
                  </div>
                ))}
              </div>
            )}
          </motion.div>

          <motion.div variants={itemVariants} className="glass-card p-6">
            <h3 className="text-lg font-bold mb-6 flex items-center gap-2 text-surface-900">
              <FileText size={20} className="text-surface-400" />
              Logs del Sistema
            </h3>
            <div className="space-y-4">
               {[
                 { msg: 'Carga inicial de datos completada', time: 'Hoy, 08:30 AM', icon: CheckCircle2, iconColor: 'text-emerald-500' },
                 { msg: 'Intento de borrado bloqueado (permisos)', time: 'Ayer, 04:15 PM', icon: AlertTriangle, iconColor: 'text-amber-500' },
                 { msg: 'Sincronización con DB realizada', time: 'Ayer, 09:00 AM', icon: Clock, iconColor: 'text-blue-500' },
               ].map((log, i) => (
                 <div key={i} className="flex gap-4 p-3 rounded-lg hover:bg-surface-50/80 transition-colors">
                   <log.icon className={`${log.iconColor} w-5 h-5 shrink-0 mt-0.5`} />
                   <div>
                     <p className="text-sm font-medium text-surface-900">{log.msg}</p>
                     <p className="text-xs text-surface-500 mt-1">{log.time}</p>
                   </div>
                 </div>
               ))}
            </div>
          </motion.div>
        </div>

        {/* Right Column: Actions */}
        <div className="space-y-6">
          <motion.div variants={itemVariants} className="glass-card p-6 border-brand-100 bg-gradient-to-br from-white to-brand-50/30">
            <h3 className="text-lg font-bold mb-2">Generación de Datos</h3>
            <p className="text-sm text-surface-500 mb-6">Genera un conjunto de datos estáticos para pruebas rápidas.</p>
            
            <button 
              onClick={() => handleAction(maintenanceApi.reset, 'Base de datos reiniciada con éxito')}
              disabled={actionLoading}
              className="w-full btn-primary py-4 flex items-center justify-center gap-2 group mb-4"
            >
              <RotateCcw className={`w-5 h-5 ${actionLoading ? 'animate-spin' : 'group-hover:rotate-180 transition-transform duration-500'}`} />
              {actionLoading ? 'Procesando...' : 'Reiniciar y Seedear'}
            </button>
            <p className="text-[11px] text-center text-surface-400 italic">
              * Esto borrará todos los datos actuales y cargará los datos de prueba predefinidos.
            </p>
          </motion.div>

          <motion.div variants={itemVariants} className="glass-card p-6 border-rose-100 bg-rose-50/20">
            <h3 className="text-lg font-bold mb-2 text-rose-900">Zona de Peligro</h3>
            <p className="text-sm text-rose-600/70 mb-6">Acciones que afectan irreversiblemente a la base de datos.</p>
            
            <button 
              onClick={() => handleAction(maintenanceApi.clear, 'Base de datos limpiada. Se recomienda resetear para poder usar la app.')}
              disabled={actionLoading}
              className="w-full py-4 px-6 rounded-xl bg-white border-2 border-rose-200 text-rose-600 font-bold hover:bg-rose-600 hover:text-white hover:border-rose-600 transition-all duration-300 flex items-center justify-center gap-2"
            >
              <Trash2 size={20} />
              Vaciar Base de Datos
            </button>
             <p className="text-[11px] text-center text-rose-400 mt-4 italic">
              Úselo con precaución. La aplicación quedará vacía.
            </p>
          </motion.div>
        </div>
      </div>
    </motion.div>
  )
}
