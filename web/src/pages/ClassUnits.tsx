import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Search, 
  BookOpen,
  RefreshCw,
  ChevronDown,
  User,
  Clock,
  Layers,
  Zap,
  Filter,
  AlertCircle,
  CheckCircle2
} from 'lucide-react'
import { classUnitsApi } from '../services/api'
import type { ClassUnit } from '../types'

const gradeLabel = (g: number) => {
  const map: Record<number, string> = {
    6: '3º Primaria', 7: '4º Primaria', 8: '5º Primaria', 9: '6º Primaria',
    10: '1º ESO', 11: '2º ESO', 12: '3º ESO', 13: '4º ESO',
    14: '1º Bachillerato', 15: '2º Bachillerato'
  }
  return map[g] || `${g}º Grado`
}

const gradeBadgeColor = (g: number) => {
  if (g <= 9) return 'bg-blue-50 text-blue-700 border-blue-200'
  if (g <= 13) return 'bg-indigo-50 text-indigo-700 border-indigo-200'
  return 'bg-purple-50 text-purple-700 border-purple-200'
}

export default function ClassUnits() {
  const [units, setUnits] = useState<ClassUnit[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [gradeFilter, setGradeFilter] = useState<number | 'all'>('all')
  const [generating, setGenerating] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null)
  const [expandedGroup, setExpandedGroup] = useState<string | null>(null)

  useEffect(() => { loadUnits() }, [])

  const loadUnits = async () => {
    try {
      const res = await classUnitsApi.getAll()
      setUnits(res.data)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const handleGenerate = async () => {
    setGenerating(true)
    setMessage(null)
    try {
      const res = await classUnitsApi.generateFromCurriculum()
      setMessage({ 
        type: 'success', 
        text: `✅ ${res.data.unitsCreated} unidades creadas para ${res.data.classroomsProcessed} aulas` 
      })
      loadUnits()
    } catch (e) {
      setMessage({ type: 'error', text: '❌ Error al generar unidades de clase' })
    } finally {
      setGenerating(false)
    }
  }

  const handleToggleActive = async (unit: ClassUnit) => {
    try {
      await classUnitsApi.update(unit.id, { ...unit, isActive: !unit.isActive })
      loadUnits()
    } catch (e) {
      console.error(e)
    }
  }

  // Get unique grades for filter
  const availableGrades = [...new Set(units.map(u => u.gradeLevel))].sort((a, b) => a - b)

  // Filter
  const filtered = units.filter(u => {
    const matchesSearch = searchTerm === '' ||
      u.subjectName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (u.teacherName || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.classroomName.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesGrade = gradeFilter === 'all' || u.gradeLevel === gradeFilter
    return matchesSearch && matchesGrade
  })

  // Group by classroom
  const groupedByClassroom = filtered.reduce<Record<string, ClassUnit[]>>((acc, unit) => {
    const key = `${unit.classroomId}`
    if (!acc[key]) acc[key] = []
    acc[key].push(unit)
    return acc
  }, {})

  const totalSessions = filtered.reduce((sum, u) => sum + u.weeklySessions, 0)
  const withTeacher = filtered.filter(u => u.teacherId).length
  const withoutTeacher = filtered.filter(u => !u.teacherId).length

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="space-y-6"
    >
      {/* Header */}
      <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-surface-900 flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-indigo-600 flex items-center justify-center">
              <BookOpen className="text-white" size={22} />
            </div>
            Unidades de Clase
          </h1>
          <p className="text-surface-500 mt-1">Gestiona las asignaciones de asignaturas, profesores y sesiones por grupo</p>
        </div>
        <button 
          onClick={handleGenerate}
          disabled={generating}
          className="btn-primary flex items-center gap-2 disabled:opacity-50"
        >
          <RefreshCw size={18} className={generating ? 'animate-spin' : ''} />
          {generating ? 'Generando...' : 'Generar desde Currículo'}
        </button>
      </div>

      {/* Message Banner */}
      <AnimatePresence>
        {message && (
          <motion.div 
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className={`p-4 rounded-xl flex items-center gap-3 ${
              message.type === 'success' 
                ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' 
                : 'bg-red-50 text-red-800 border border-red-200'
            }`}
          >
            {message.type === 'success' ? <CheckCircle2 size={20} /> : <AlertCircle size={20} />}
            <span className="font-medium">{message.text}</span>
            <button onClick={() => setMessage(null)} className="ml-auto text-sm font-bold hover:underline">Cerrar</button>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Stats Row */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[
          { label: 'Unidades Totales', value: filtered.length, icon: BookOpen, color: 'text-indigo-600 bg-indigo-50' },
          { label: 'Sesiones/Semana', value: totalSessions, icon: Clock, color: 'text-blue-600 bg-blue-50' },
          { label: 'Con Profesor', value: withTeacher, icon: User, color: 'text-emerald-600 bg-emerald-50' },
          { label: 'Sin Asignar', value: withoutTeacher, icon: AlertCircle, color: 'text-amber-600 bg-amber-50' },
        ].map((stat, i) => (
          <motion.div 
            key={i}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.05 }}
            className="glass-card p-4 flex items-center gap-4"
          >
            <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${stat.color}`}>
              <stat.icon size={20} />
            </div>
            <div>
              <p className="text-2xl font-bold text-surface-900">{stat.value}</p>
              <p className="text-xs text-surface-500 font-medium">{stat.label}</p>
            </div>
          </motion.div>
        ))}
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-surface-400" size={18} />
          <input 
            type="text" 
            placeholder="Buscar asignatura, profesor o grupo..." 
            className="input-field w-full pl-10"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <div className="relative">
          <Filter className="absolute left-3 top-1/2 -translate-y-1/2 text-surface-400" size={16} />
          <select 
            value={gradeFilter}
            onChange={(e) => setGradeFilter(e.target.value === 'all' ? 'all' : Number(e.target.value))}
            className="input-field pl-10 pr-8 min-w-[200px]"
          >
            <option value="all">Todos los niveles</option>
            {availableGrades.map(g => (
              <option key={g} value={g}>{gradeLabel(g)}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Content */}
      <div className="space-y-3">
        {loading ? (
          <div className="glass-card p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
            <p className="text-surface-500 font-medium">Cargando unidades de clase...</p>
          </div>
        ) : Object.keys(groupedByClassroom).length === 0 ? (
          <div className="glass-card p-12 text-center space-y-4">
            <Layers className="mx-auto text-surface-300" size={48} />
            <div>
              <p className="text-lg font-semibold text-surface-600">No hay unidades de clase</p>
              <p className="text-surface-400 mt-1">Genera las unidades desde el currículo para empezar</p>
            </div>
            <button onClick={handleGenerate} className="btn-primary mx-auto flex items-center gap-2">
              <Zap size={18} />
              Generar Automáticamente
            </button>
          </div>
        ) : (
          Object.entries(groupedByClassroom).map(([classroomId, classUnits]) => {
            const first = classUnits[0]
            const isExpanded = expandedGroup === classroomId
            const totalHours = classUnits.reduce((s, u) => s + u.weeklySessions, 0)
            const unassigned = classUnits.filter(u => !u.teacherId).length

            return (
              <motion.div 
                key={classroomId}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                className="glass-card overflow-hidden"
              >
                {/* Classroom Header */}
                <button 
                  onClick={() => setExpandedGroup(isExpanded ? null : classroomId)}
                  className="w-full px-6 py-4 flex items-center justify-between hover:bg-surface-50/50 transition-colors"
                >
                  <div className="flex items-center gap-4">
                    <span className={`px-3 py-1 rounded-lg text-xs font-bold border ${gradeBadgeColor(first.gradeLevel)}`}>
                      {gradeLabel(first.gradeLevel)}
                    </span>
                    <span className="px-2 py-0.5 rounded bg-surface-100 text-surface-700 font-bold text-sm">
                      Línea {first.line}
                    </span>
                    <span className="text-surface-500 text-sm">
                      {classUnits.length} asignaturas · {totalHours}h/semana
                      {unassigned > 0 && (
                        <span className="ml-2 text-amber-600 font-semibold">
                          ({unassigned} sin profesor)
                        </span>
                      )}
                    </span>
                  </div>
                  <ChevronDown 
                    size={20} 
                    className={`text-surface-400 transition-transform ${isExpanded ? 'rotate-180' : ''}`} 
                  />
                </button>

                {/* Expanded Content */}
                <AnimatePresence>
                  {isExpanded && (
                    <motion.div
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: 'auto', opacity: 1 }}
                      exit={{ height: 0, opacity: 0 }}
                      transition={{ duration: 0.2 }}
                      className="overflow-hidden"
                    >
                      <div className="border-t border-surface-100">
                        <table className="w-full text-left">
                          <thead>
                            <tr className="bg-surface-50">
                              <th className="px-6 py-3 text-xs font-bold text-surface-500 uppercase tracking-wider">Asignatura</th>
                              <th className="px-6 py-3 text-xs font-bold text-surface-500 uppercase tracking-wider">Profesor</th>
                              <th className="px-6 py-3 text-xs font-bold text-surface-500 uppercase tracking-wider text-center">Sesiones</th>
                              <th className="px-6 py-3 text-xs font-bold text-surface-500 uppercase tracking-wider text-center">Opciones</th>
                              <th className="px-6 py-3 text-xs font-bold text-surface-500 uppercase tracking-wider text-center">Estado</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-surface-100">
                            {classUnits
                              .sort((a, b) => b.weeklySessions - a.weeklySessions)
                              .map((unit) => (
                              <tr key={unit.id} className="hover:bg-surface-50/50 transition-colors group">
                                <td className="px-6 py-3">
                                  <div className="flex items-center gap-3">
                                    <div 
                                      className="w-3 h-3 rounded-full shrink-0"
                                      style={{ backgroundColor: unit.subjectColor }}
                                    />
                                    <span className="font-medium text-surface-900 text-sm">{unit.subjectName}</span>
                                  </div>
                                </td>
                                <td className="px-6 py-3">
                                  {unit.teacherName ? (
                                    <div className="flex items-center gap-2">
                                      <div className="w-6 h-6 rounded-full bg-brand-100 flex items-center justify-center">
                                        <User size={12} className="text-brand-600" />
                                      </div>
                                      <span className="text-sm text-surface-700">{unit.teacherName}</span>
                                    </div>
                                  ) : (
                                    <span className="text-xs text-amber-600 font-medium bg-amber-50 px-2 py-1 rounded-lg">
                                      Sin asignar
                                    </span>
                                  )}
                                </td>
                                <td className="px-6 py-3 text-center">
                                  <span className="text-sm font-bold text-surface-900">{unit.weeklySessions}h</span>
                                  {unit.sessionDuration > 1 && (
                                    <span className="ml-1 text-[10px] bg-indigo-50 text-indigo-600 px-1.5 py-0.5 rounded font-bold">
                                      DOBLE
                                    </span>
                                  )}
                                </td>
                                <td className="px-6 py-3">
                                  <div className="flex items-center justify-center gap-1.5">
                                    {unit.preferNonConsecutive && (
                                      <span className="text-[10px] bg-surface-100 text-surface-600 px-1.5 py-0.5 rounded font-medium" title="Evitar días consecutivos">
                                        ≠Consec
                                      </span>
                                    )}
                                    {unit.allowDoubleSession && (
                                      <span className="text-[10px] bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded font-medium" title="Permite sesión doble">
                                        2×
                                      </span>
                                    )}
                                    {unit.preferredRoomName && (
                                      <span className="text-[10px] bg-purple-50 text-purple-600 px-1.5 py-0.5 rounded font-medium" title={`Aula: ${unit.preferredRoomName}`}>
                                        🏫
                                      </span>
                                    )}
                                  </div>
                                </td>
                                <td className="px-6 py-3 text-center">
                                  <button
                                    onClick={() => handleToggleActive(unit)}
                                    className={`text-xs font-bold px-3 py-1 rounded-full transition-colors ${
                                      unit.isActive 
                                        ? 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100' 
                                        : 'bg-surface-100 text-surface-400 hover:bg-surface-200'
                                    }`}
                                  >
                                    {unit.isActive ? 'Activa' : 'Inactiva'}
                                  </button>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.div>
            )
          })
        )}
      </div>
    </motion.div>
  )
}
