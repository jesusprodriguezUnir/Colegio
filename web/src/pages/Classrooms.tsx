import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Plus, 
  Search, 
  Edit2, 
  Trash2, 
  DoorOpen, 
  School,
  X,
  UserSquare2,
  Settings
} from 'lucide-react'
import { classroomsApi } from '../services/api'
import type { Classroom } from '../types'

export default function Classrooms() {
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<Classroom | null>(null)
  const [form, setForm] = useState({ gradeLevel: 1, line: 'A', schoolId: '', tutorId: '' })
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    loadClassrooms()
  }, [])

  const loadClassrooms = async () => {
    try {
      const res = await classroomsApi.getAll()
      setClassrooms(res.data)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editing) {
        await classroomsApi.update(editing.id, form)
      } else {
        await classroomsApi.create(form)
      }
      resetForm()
      loadClassrooms()
    } catch (e) {
      console.error(e)
    }
  }

  const resetForm = () => {
    setForm({ gradeLevel: 1, line: 'A', schoolId: '', tutorId: '' })
    setShowForm(false)
    setEditing(null)
  }

  const handleDelete = async (id: string) => {
    if (confirm('¿Estás seguro de que deseas eliminar esta aula?')) {
      try {
        await classroomsApi.delete(id)
        loadClassrooms()
      } catch (e) {
        console.error(e)
      }
    }
  }

  const handleEdit = (classroom: Classroom) => {
    setEditing(classroom)
    setForm({ 
      gradeLevel: classroom.gradeLevel, 
      line: classroom.line, 
      schoolId: classroom.schoolId, 
      tutorId: classroom.tutorId || '' 
    })
    setShowForm(true)
  }

  const gradeLabel = (g: number) => {
    if (g <= 3) return `${g}º Infantil`
    if (g <= 9) return `${g - 3}º Primaria`
    return `${g - 9}º ESO`
  }

  const filteredClassrooms = classrooms.filter(c => 
    gradeLabel(c.gradeLevel).toLowerCase().includes(searchTerm.toLowerCase()) ||
    c.line.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="space-y-6"
    >
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-surface-400" size={18} />
          <input 
            type="text" 
            placeholder="Filtrar por nivel o línea..." 
            className="input-field w-full pl-10"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <button 
          onClick={() => { setShowForm(true); setEditing(null); }} 
          className="btn-primary flex items-center gap-2 w-full sm:w-auto justify-center"
        >
          <Plus size={20} />
          Nueva Aula
        </button>
      </div>

      <div className="glass-card overflow-hidden">
        {loading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
            <p className="text-surface-500 font-medium">Cargando aulas...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-surface-50 border-b border-surface-200">
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Aula / Nivel</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Línea</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Centro</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-100">
                <AnimatePresence>
                  {filteredClassrooms.map((c) => (
                    <motion.tr 
                      key={c.id}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      className="hover:bg-surface-50/50 transition-colors group"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-xl bg-orange-50 flex items-center justify-center text-orange-600 font-bold">
                            <DoorOpen size={20} />
                          </div>
                          <span className="font-semibold text-surface-900">{gradeLabel(c.gradeLevel)}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <span className="px-3 py-1 rounded-lg bg-surface-100 text-surface-700 font-bold text-sm">
                          {c.line}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-sm text-surface-600">
                        <div className="flex items-center gap-1.5">
                          <School size={14} className="text-surface-400" />
                          {c.schoolId.split('-')[0]}...
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button 
                            onClick={() => handleEdit(c)}
                            className="p-2 hover:bg-brand-50 text-brand-600 rounded-lg transition-colors"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            onClick={() => handleDelete(c.id)}
                            className="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
                {!loading && filteredClassrooms.length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-6 py-12 text-center text-surface-400">
                      No se encontraron aulas.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <AnimatePresence>
        {showForm && (
          <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={resetForm}
              className="absolute inset-0 bg-surface-900/40 backdrop-blur-sm"
            />
            <motion.div 
              initial={{ scale: 0.95, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.95, opacity: 0, y: 20 }}
              className="bg-white rounded-2xl shadow-2xl w-full max-w-lg relative z-10 overflow-hidden"
            >
              <div className="p-6 border-b border-surface-100 flex items-center justify-between bg-surface-50">
                <h3 className="text-xl font-bold">{editing ? 'Editar Aula' : 'Nueva Aula'}</h3>
                <button onClick={resetForm} className="p-2 hover:bg-surface-200 rounded-full transition-colors">
                  <X size={20} />
                </button>
              </div>
              <form onSubmit={handleSubmit} className="p-8 space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-surface-700">Nivel Educativo</label>
                    <select 
                      value={form.gradeLevel} 
                      onChange={e => setForm({ ...form, gradeLevel: Number(e.target.value) })} 
                      className="input-field w-full"
                    >
                      {[1,2,3,4,5,6,7,8,9,10,11,12].map(g => <option key={g} value={g}>{gradeLabel(g)}</option>)}
                    </select>
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-surface-700">Línea</label>
                    <select 
                      value={form.line} 
                      onChange={e => setForm({ ...form, line: e.target.value })} 
                      className="input-field w-full"
                    >
                      <option value="A">Línea A</option>
                      <option value="B">Línea B</option>
                      <option value="C">Línea C</option>
                    </select>
                  </div>
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-surface-700">Centro Educativo (ID)</label>
                  <input 
                    value={form.schoolId} 
                    onChange={e => setForm({ ...form, schoolId: e.target.value })} 
                    placeholder="ID del Colegio" 
                    className="input-field w-full" 
                  />
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-surface-700">Tutor Principal (ID - Opcional)</label>
                  <input 
                    value={form.tutorId} 
                    onChange={e => setForm({ ...form, tutorId: e.target.value })} 
                    placeholder="ID del Tutor" 
                    className="input-field w-full" 
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <button type="button" onClick={resetForm} className="btn-secondary flex-1">Cancelar</button>
                  <button type="submit" className="btn-primary flex-1">
                    {editing ? 'Guardar Cambios' : 'Crear Aula'}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </motion.div>
  )
}