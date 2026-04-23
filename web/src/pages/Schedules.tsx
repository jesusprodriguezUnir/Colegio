import React, { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Calendar, 
  RefreshCw, 
  Save, 
  ChevronRight, 
  Filter,
  AlertCircle,
  CheckCircle2,
  Settings2
} from 'lucide-react'
import { classroomsApi, schedulesApi, timeSlotsApi } from '../services/api'
import { Classroom, Schedule, TimeSlot, AcademicSessionType } from '../types'
import TimetableGrid from '../components/TimetableGrid'

export default function Schedules() {
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [selectedClassroom, setSelectedClassroom] = useState<string>('')
  const [sessionType, setSessionType] = useState<AcademicSessionType>(AcademicSessionType.Standard)
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([])
  const [schedules, setSchedules] = useState<Schedule[]>([])
  const [loading, setLoading] = useState(false)
  const [generating, setGenerating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  useEffect(() => {
    fetchClassrooms()
    fetchTimeSlots()
  }, [])

  useEffect(() => {
    if (selectedClassroom) {
      fetchSchedules()
    }
  }, [selectedClassroom, sessionType])

  const fetchClassrooms = async () => {
    try {
      const res = await classroomsApi.getAll()
      setClassrooms(res.data)
      if (res.data.length > 0) setSelectedClassroom(res.data[0].id)
    } catch (err) {
      console.error('Error fetching classrooms:', err)
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
      const res = await schedulesApi.getByClassroom(selectedClassroom)
      // Filter by session type
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
    setGenerating(true)
    setError(null)
    setSuccess(null)
    try {
      const res = await schedulesApi.generate(selectedClassroom, sessionType)
      setSchedules(res.data)
      setSuccess('¡Horario generado con éxito!')
      setTimeout(() => setSuccess(null), 3000)
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al generar el horario')
    } finally {
      setGenerating(false)
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
      className="max-w-[1600px] mx-auto space-y-8"
    >
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 bg-brand-100 rounded-2xl flex items-center justify-center text-brand-600 shadow-sm border border-brand-200">
            <Calendar size={24} />
          </div>
          <div>
            <h1 className="text-3xl font-display font-bold text-surface-900 tracking-tight">Gestión de Horarios</h1>
            <p className="text-surface-500">Configura y genera automáticamente los horarios del centro</p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <button 
            onClick={handleGenerate}
            disabled={generating || !selectedClassroom}
            className="flex items-center gap-2 px-6 py-3 bg-brand-600 text-white rounded-xl font-semibold hover:bg-brand-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-md shadow-brand-200"
          >
            {generating ? (
              <RefreshCw className="animate-spin" size={18} />
            ) : (
              <RefreshCw size={18} />
            )}
            Generar Automáticamente
          </button>
        </div>
      </div>

      {/* Controls Card */}
      <div className="bg-white p-6 rounded-3xl border border-surface-200 shadow-sm flex flex-wrap items-center gap-8">
        <div className="flex items-center gap-4">
          <div className="p-2 bg-surface-50 rounded-lg text-surface-500">
            <Settings2 size={20} />
          </div>
          <div className="flex flex-col">
            <label className="text-[10px] font-bold text-surface-400 uppercase tracking-wider mb-1">Aula / Clase</label>
            <select 
              value={selectedClassroom}
              onChange={(e) => setSelectedClassroom(e.target.value)}
              className="bg-transparent font-semibold text-surface-900 border-none p-0 focus:ring-0 cursor-pointer"
            >
              {classrooms.map(c => (
                <option key={c.id} value={c.id}>
                  {c.gradeLevel}º {c.line}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="h-10 w-px bg-surface-200 hidden md:block" />

        <div className="flex items-center gap-4">
          <div className="p-2 bg-surface-50 rounded-lg text-surface-500">
            <Filter size={20} />
          </div>
          <div className="flex flex-col">
            <label className="text-[10px] font-bold text-surface-400 uppercase tracking-wider mb-1">Periodo Académico</label>
            <div className="flex bg-surface-100 p-1 rounded-lg">
              <button 
                onClick={() => setSessionType(AcademicSessionType.Standard)}
                className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all ${
                  sessionType === AcademicSessionType.Standard 
                    ? 'bg-white text-brand-600 shadow-sm' 
                    : 'text-surface-500 hover:text-surface-700'
                }`}
              >
                Estándar (Oct-May)
              </button>
              <button 
                onClick={() => setSessionType(AcademicSessionType.Intensive)}
                className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all ${
                  sessionType === AcademicSessionType.Intensive 
                    ? 'bg-white text-brand-600 shadow-sm' 
                    : 'text-surface-500 hover:text-surface-700'
                }`}
              >
                Intensivo (Jun/Sep)
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Notifications */}
      <AnimatePresence>
        {error && (
          <motion.div 
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            className="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-xl flex items-center gap-3"
          >
            <AlertCircle size={18} />
            <span className="text-sm font-medium">{error}</span>
          </motion.div>
        )}
        {success && (
          <motion.div 
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            className="bg-green-50 border border-green-200 text-green-600 px-4 py-3 rounded-xl flex items-center gap-3"
          >
            <CheckCircle2 size={18} />
            <span className="text-sm font-medium">{success}</span>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Grid Section */}
      <div className="relative min-h-[400px]">
        {loading && (
          <div className="absolute inset-0 bg-white/50 backdrop-blur-sm z-10 flex items-center justify-center rounded-3xl">
            <RefreshCw className="animate-spin text-brand-600" size={32} />
          </div>
        )}
        <TimetableGrid 
          timeSlots={filteredTimeSlots}
          schedules={schedules}
          onToggleLock={handleToggleLock}
        />
      </div>
    </motion.div>
  )
}
