import { useState, useEffect } from 'react'
import { motion } from 'framer-motion'
import { 
  Users, 
  School, 
  SquareUserRound, 
  TrendingUp, 
  ArrowUpRight, 
  Calendar,
  MoreVertical,
  CheckCircle2,
  Clock
} from 'lucide-react'
import { 
  AreaChart, 
  Area, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer
} from 'recharts'
import { schoolsApi, teachersApi, studentsApi, invoicesApi } from '../services/api'

const chartData = [
  { name: 'Ene', ingresos: 45000, asistencia: 95 },
  { name: 'Feb', ingresos: 52000, asistencia: 92 },
  { name: 'Mar', ingresos: 48000, asistencia: 98 },
  { name: 'Abr', ingresos: 61000, asistencia: 96 },
  { name: 'May', ingresos: 55000, asistencia: 94 },
  { name: 'Jun', ingresos: 67000, asistencia: 97 },
]

const recentActivity = [
  { id: 1, type: 'enrollment', user: 'Laura Martínez', detail: 'Nueva matrícula en 2º Primaria', time: 'hace 2 horas', status: 'success' },
  { id: 2, type: 'payment', user: 'Carlos Ruiz', detail: 'Pago de mensualidad recibido', time: 'hace 5 horas', status: 'success' },
  { id: 3, type: 'alert', user: 'Sistema', detail: 'Recordatorio de tutoría enviado', time: 'hace 1 día', status: 'pending' },
]

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1
    }
  }
}

const itemVariants = {
  hidden: { y: 20, opacity: 0 },
  visible: { y: 0, opacity: 1 }
}

export default function Dashboard() {
  const [stats, setStats] = useState({ schools: 0, teachers: 0, students: 0, revenue: 0 })
  const [, setLoading] = useState(true)

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const [sRes, tRes, stRes, iRes] = await Promise.all([
          schoolsApi.getAll(),
          teachersApi.getAll(),
          studentsApi.getAll(),
          invoicesApi.getAll()
        ])
        
        const totalRevenue = iRes.data
          .filter((inv: any) => inv.status === 'Paid')
          .reduce((sum: number, inv: any) => sum + inv.totalAmount, 0)

        setStats({
          schools: sRes.data.length,
          teachers: tRes.data.length,
          students: stRes.data.length,
          revenue: totalRevenue
        })
      } catch (e) {
        console.error('Error fetching stats', e)
      } finally {
        setLoading(false)
      }
    }
    fetchStats()
  }, [])

  const statCards = [
    { label: 'Alumnos Totales', value: stats.students, icon: Users, color: 'text-blue-600', bg: 'bg-blue-50', trend: '+12%' },
    { label: 'Cuerpo Docente', value: stats.teachers, icon: SquareUserRound, color: 'text-indigo-600', bg: 'bg-indigo-50', trend: '+5%' },
    { label: 'Centros Activos', value: stats.schools, icon: School, color: 'text-purple-600', bg: 'bg-purple-50', trend: '0%' },
    { label: 'Ingresos (Mes)', value: `€${stats.revenue.toLocaleString()}`, icon: TrendingUp, color: 'text-emerald-600', bg: 'bg-emerald-50', trend: '+8.4%' },
  ]

  return (
    <motion.div 
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="space-y-8"
    >
      {/* Welcome Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold text-surface-900">Hola, Administrador 👋</h2>
          <p className="text-surface-500 mt-1">Aquí tienes el resumen de hoy en tus instituciones.</p>
        </div>
        <div className="flex gap-3">
          <button className="btn-secondary flex items-center gap-2">
            <Calendar size={18} />
            Enero 2026
          </button>
          <button className="btn-primary">Generar Informe</button>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statCards.map((card, idx) => (
          <motion.div 
            key={idx}
            variants={itemVariants}
            whileHover={{ y: -5 }}
            className="glass-card p-6 flex items-center gap-5 group"
          >
            <div className={`w-14 h-14 rounded-2xl ${card.bg} flex items-center justify-center shrink-0 transition-transform group-hover:scale-110`}>
              <card.icon className={`${card.color} w-7 h-7`} />
            </div>
            <div className="flex-1 overflow-hidden">
              <p className="text-sm font-medium text-surface-500">{card.label}</p>
              <div className="flex items-baseline gap-2">
                <h3 className="text-2xl font-bold text-surface-900">{card.value}</h3>
                <span className="text-xs font-bold text-emerald-600 flex items-center">
                  <ArrowUpRight size={12} />
                  {card.trend}
                </span>
              </div>
            </div>
          </motion.div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Chart */}
        <motion.div variants={itemVariants} className="lg:col-span-2 glass-card p-8">
          <div className="flex items-center justify-between mb-8">
            <div>
              <h3 className="text-lg font-bold">Rendimiento Financiero</h3>
              <p className="text-sm text-surface-500">Ingresos brutos mensuales vs Periodo anterior</p>
            </div>
            <button className="p-2 hover:bg-surface-50 rounded-lg text-surface-400">
              <MoreVertical size={20} />
            </button>
          </div>
          <div className="h-[350px] w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={chartData}>
                <defs>
                  <linearGradient id="colorIngresos" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#3b54f6" stopOpacity={0.1}/>
                    <stop offset="95%" stopColor="#3b54f6" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                <XAxis 
                  dataKey="name" 
                  axisLine={false} 
                  tickLine={false} 
                  tick={{fill: '#94a3b8', fontSize: 12}}
                  dy={10}
                />
                <YAxis 
                  axisLine={false} 
                  tickLine={false} 
                  tick={{fill: '#94a3b8', fontSize: 12}}
                  tickFormatter={(val) => `€${val/1000}k`}
                />
                <Tooltip 
                  contentStyle={{borderRadius: '12px', border: 'none', boxShadow: '0 10px 15px -3px rgba(0,0,0,0.1)'}}
                />
                <Area 
                  type="monotone" 
                  dataKey="ingresos" 
                  stroke="#3b54f6" 
                  strokeWidth={3}
                  fillOpacity={1} 
                  fill="url(#colorIngresos)" 
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </motion.div>

        {/* Recent Activity */}
        <motion.div variants={itemVariants} className="glass-card p-8 bg-surface-900 text-white border-none shadow-premium">
          <h3 className="text-lg font-bold mb-6">Actividad Reciente</h3>
          <div className="space-y-6">
            {recentActivity.map((act) => (
              <div key={act.id} className="flex gap-4 group cursor-pointer">
                <div className="shrink-0 mt-1">
                  {act.status === 'success' ? (
                    <div className="w-8 h-8 rounded-full bg-emerald-500/20 flex items-center justify-center">
                      <CheckCircle2 size={16} className="text-emerald-400" />
                    </div>
                  ) : (
                    <div className="w-8 h-8 rounded-full bg-amber-500/20 flex items-center justify-center">
                      <Clock size={16} className="text-amber-400" />
                    </div>
                  )}
                </div>
                <div className="flex-1 border-b border-surface-800 pb-4 group-last:border-none">
                  <p className="text-sm font-semibold">{act.user}</p>
                  <p className="text-xs text-surface-400 mt-0.5">{act.detail}</p>
                  <p className="text-[10px] text-surface-500 mt-2 uppercase tracking-wider font-bold">{act.time}</p>
                </div>
              </div>
            ))}
          </div>
          <button className="w-full mt-6 py-3 rounded-xl bg-white/10 hover:bg-white/15 transition-colors text-sm font-semibold">
            Ver Todo el Historial
          </button>
        </motion.div>
      </div>
    </motion.div>
  )
}
