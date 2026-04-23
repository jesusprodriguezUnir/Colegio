import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Calendar, 
  RefreshCw, 
  Filter,
  AlertCircle,
  CheckCircle2,
  Settings2,
  Users,
  MapPin,
  School as SchoolIcon,
  Download
} from 'lucide-react'
import { classroomsApi, schedulesApi, timeSlotsApi, teachersApi, roomsApi } from '../services/api'
import { 
  type Classroom, 
  type Schedule, 
  type TimeSlot, 
  type Teacher, 
  type Room,
  type AcademicSessionType, 
  AcademicSession 
} from '../types'
import TimetableGrid from '../components/TimetableGrid'

type ViewMode = 'classroom' | 'teacher' | 'room'

export default function Schedules() {
  const [viewMode, setViewMode] = useState<ViewMode>('classroom')
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [teachers, setTeachers] = useState<Teacher[]>([])
  const [rooms, setRooms] = useState<Room[]>([])
  
  const [selectedId, setSelectedId] = useState<string>('')
  const [sessionType, setSessionType] = useState<AcademicSessionType>(AcademicSession.Standard)
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([])
  const [schedules, setSchedules] = useState<Schedule[]>([])
  
  const [loading, setLoading] = useState(false)
  const [generating, setGenerating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  useEffect(() => {
    fetchInitialData()
    fetchTimeSlots()
  }, [])

  useEffect(() => {
    if (selectedId) {
      fetchSchedules()
    } else {
      setSchedules([])
    }
  }, [selectedId, sessionType, viewMode])

  const fetchInitialData = async () => {
    try {
      const [cRes, tRes, rRes] = await Promise.all([
        classroomsApi.getAll(),
        teachersApi.getAll(),
        roomsApi.getAll()
      ])
      setClassrooms(cRes.data)
      setTeachers(tRes.data)
      setRooms(rRes.data)
      
      if (cRes.data.length > 0) setSelectedId(cRes.data[0].id)
    } catch (err) {
      console.error('Error fetching initial data:', err)
    }
  }

  const fetchTimeSlots = async () => {
    try {
      const res = await timeSlotsApi.getAll()
      setTimeSlots(res.data)
    } catch (err) {
      console.error('Error fetching time slots:', err)
    }
  }

  const fetchSchedules = async () => {
    setLoading(true)
    setError(null)
    try {
      let res;
      if (viewMode === 'classroom') {
        res = await schedulesApi.getByClassroom(selectedId)
      } else if (viewMode === 'teacher') {
        res = await schedulesApi.getByTeacher(selectedId)
      } else {
        res = await schedulesApi.getByRoom(selectedId)
      }
      
      const filtered = res.data.filter((s: Schedule) => {
        const slot = timeSlots.find(ts => ts.id === s.timeSlotId)
        return slot?.sessionType === sessionType
      })
      setSchedules(filtered)
    } catch (err) {
      setError('Error al cargar el horario')
    } finally {
      setLoading(false)
    }
  }

  const handleGenerate = async () => {
    if (viewMode !== 'classroom') return
    
    setGenerating(true)
    setError(null)
    setSuccess(null)
    try {
      const res = await schedulesApi.generate(selectedId, sessionType)
      // The generate endpoint returns the new schedules
      setSchedules(res.data.schedules)
      setSuccess('¡Horario generado con éxito!')
      setTimeout(() => setSuccess(null), 3000)
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al generar el horario')
    } finally {
      setGenerating(false)
    }
  }

  const handleUpdateSchedule = async (scheduleId: string, newTimeSlotId: string) => {
    const schedule = schedules.find(s => s.id === scheduleId)
    if (!schedule) return

    const originalTimeSlotId = schedule.timeSlotId
    
    // Optimistic update
    setSchedules(prev => prev.map(s => s.id === scheduleId ? { ...s, timeSlotId: newTimeSlotId } : s))

    try {
      await schedulesApi.update(scheduleId, { ...schedule, timeSlotId: newTimeSlotId })
      // Refresh to ensure everything is consistent (e.g. navigation properties)
      fetchSchedules()
    } catch (err) {
      // Rollback
      setSchedules(prev => prev.map(s => s.id === scheduleId ? { ...s, timeSlotId: originalTimeSlotId } : s))
      setError('Conflicto detectado. No se pudo mover la sesión.')
    }
  }

  const handleToggleLock = async (schedule: Schedule) => {
    try {
      const updated = { ...schedule, isLocked: !schedule.isLocked }
      await schedulesApi.update(schedule.id, updated)
      setSchedules(prev => prev.map(s => s.id === schedule.id ? updated : s))
    } catch (err) {
      setError('Error al actualizar el estado de bloqueo')
    }
  }

  const filteredTimeSlots = timeSlots.filter(ts => ts.sessionType === sessionType)

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="max-w-[1600px] mx-auto space-y-8 pb-20"
    >
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 bg-brand-600 rounded-2xl flex items-center justify-center text-white shadow-lg shadow-brand-200">
            <Calendar size={24} />
          </div>
          <div>
            <h1 className="text-3xl font-display font-bold text-surface-900 tracking-tight">Editor de Horarios</h1>
            <p className="text-surface-500 font-medium">Visualiza y ajusta la planificación del centro</p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <button className="flex items-center gap-2 px-4 py-2.5 bg-white text-surface-700 border border-surface-200 rounded-xl font-semibold hover:bg-surface-50 transition-all shadow-sm">
            <Download size={18} />
            Exportar PDF
          </button>
          
          {viewMode === 'classroom' && (
            <button 
              onClick={handleGenerate}
              disabled={generating || !selectedId}
              className="flex items-center gap-2 px-6 py-2.5 bg-brand-600 text-white rounded-xl font-semibold hover:bg-brand-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-md shadow-brand-200"
            >
              {generating ? <RefreshCw className="animate-spin" size={18} /> : <RefreshCw size={18} />}
              Generar
            </button>
          )}
        </div>
      </div>

      {/* Main Controls Card */}
      <div className="bg-white p-2 rounded-[2rem] border border-surface-200 shadow-xl shadow-surface-100/50 flex flex-col md:flex-row gap-2">
        {/* View Mode Tabs */}
        <div className="flex bg-surface-100 p-1.5 rounded-[1.5rem] md:w-fit">
          <button 
            onClick={() => { setViewMode('classroom'); if (classrooms[0]) setSelectedId(classrooms[0].id) }}
            className={`flex items-center gap-2 px-6 py-2.5 rounded-2xl text-sm font-bold transition-all ${
              viewMode === 'classroom' ? 'bg-white text-brand-600 shadow-sm' : 'text-surface-500 hover:text-surface-700'
            }`}
          >
            <SchoolIcon size={16} />
            Aulas
          </button>
          <button 
            onClick={() => { setViewMode('teacher'); if (teachers[0]) setSelectedId(teachers[0].id) }}
            className={`flex items-center gap-2 px-6 py-2.5 rounded-2xl text-sm font-bold transition-all ${
              viewMode === 'teacher' ? 'bg-white text-brand-600 shadow-sm' : 'text-surface-500 hover:text-surface-700'
            }`}
          >
            <Users size={16} />
            Profesores
          </button>
          <button 
            onClick={() => { setViewMode('room'); if (rooms[0]) setSelectedId(rooms[0].id) }}
            className={`flex items-center gap-2 px-6 py-2.5 rounded-2xl text-sm font-bold transition-all ${
              viewMode === 'room' ? 'bg-white text-brand-600 shadow-sm' : 'text-surface-500 hover:text-surface-700'
            }`}
          >
            <MapPin size={16} />
            Espacios
          </button>
        </div>

        <div className="flex-1 flex flex-wrap items-center gap-4 px-4 py-2">
          {/* Entity Selector */}
          <div className="flex items-center gap-3 min-w-[200px]">
            <Settings2 size={18} className="text-surface-400" />
            <select 
              value={selectedId}
              onChange={(e) => setSelectedId(e.target.value)}
              className="flex-1 bg-transparent font-bold text-surface-900 border-none p-0 focus:ring-0 cursor-pointer text-lg"
            >
              {viewMode === 'classroom' && classrooms.map(c => (
                <option key={c.id} value={c.id}>{c.gradeLevel}º {c.line}</option>
              ))}
              {viewMode === 'teacher' && teachers.map(t => (
                <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>
              ))}
              {viewMode === 'room' && rooms.map(r => (
                <option key={r.id} value={r.id}>{r.name}</option>
              ))}
            </select>
          </div>

          <div className="h-8 w-px bg-surface-200 hidden lg:block" />

          {/* Session Switcher */}
          <div className="flex items-center gap-3">
            <Filter size={18} className="text-surface-400" />
            <div className="flex gap-1">
              <button 
                onClick={() => setSessionType(AcademicSession.Standard)}
                className={`px-4 py-1.5 rounded-xl text-xs font-bold transition-all border ${
                  sessionType === AcademicSession.Standard 
                    ? 'bg-brand-50 text-brand-600 border-brand-200' 
                    : 'bg-white text-surface-500 border-surface-200 hover:border-surface-300'
                }`}
              >
                Estándar
              </button>
              <button 
                onClick={() => setSessionType(AcademicSession.Intensive)}
                className={`px-4 py-1.5 rounded-xl text-xs font-bold transition-all border ${
                  sessionType === AcademicSession.Intensive 
                    ? 'bg-brand-50 text-brand-600 border-brand-200' 
                    : 'bg-white text-surface-500 border-surface-200 hover:border-surface-300'
                }`}
              >
                Intensivo
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Notifications */}
      <AnimatePresence>
        {error && (
          <motion.div 
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className="bg-red-50 border border-red-200 text-red-600 px-5 py-4 rounded-2xl flex items-center justify-between shadow-sm"
          >
            <div className="flex items-center gap-3">
              <AlertCircle size={20} />
              <span className="font-semibold">{error}</span>
            </div>
            <button onClick={() => setError(null)} className="text-red-400 hover:text-red-600">✕</button>
          </motion.div>
        )}
        {success && (
          <motion.div 
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className="bg-green-50 border border-green-200 text-green-600 px-5 py-4 rounded-2xl flex items-center gap-3 shadow-sm"
          >
            <CheckCircle2 size={20} />
            <span className="font-semibold">{success}</span>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Grid Section */}
      <div className="relative">
        <TimetableGrid 
          timeSlots={filteredTimeSlots}
          schedules={schedules}
          onToggleLock={handleToggleLock}
          onMoveSchedule={handleUpdateSchedule}
          viewMode={viewMode}
          loading={loading}
        />
      </div>
    </motion.div>
  )
}
