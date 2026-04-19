import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Plus, 
  Search, 
  Edit2, 
  Trash2, 
  BookOpen, 
  Calendar,
  X,
  UserSquare2,
  MoreVertical
} from 'lucide-react'
import { teachersApi } from '../services/api'
import type { Teacher } from '../types'

export default function Teachers() {
  const [teachers, setTeachers] = useState<Teacher[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<Teacher | null>(null)
  const [form, setForm] = useState({ firstName: '', lastName: '', specialty: '', hireDate: '' })
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    loadTeachers()
  }, [])

  const loadTeachers = async () => {
    try {
      const res = await teachersApi.getAll()
      setTeachers(res.data)
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
        await teachersApi.update(editing.id, form)
      } else {
        await teachersApi.create(form)
      }
      resetForm()
      loadTeachers()
    } catch (e) {
      console.error(e)
    }
  }

  const resetForm = () => {
    setForm({ firstName: '', lastName: '', specialty: '', hireDate: '' })
    setShowForm(false)
    setEditing(null)
  }

  const handleDelete = async (id: string) => {
    if (confirm('¿Deseas eliminar permanentemente a este profesor?')) {
      try {
        await teachersApi.delete(id)
        loadTeachers()
      } catch (e) {
        console.error(e)
      }
    }
  }

  const handleEdit = (teacher: Teacher) => {
    setEditing(teacher)
    setForm({ firstName: teacher.firstName, lastName: teacher.lastName, specialty: teacher.specialty, hireDate: teacher.hireDate })
    setShowForm(true)
  }

  const filteredTeachers = teachers.filter(t => 
    (t.firstName + ' ' + t.lastName).toLowerCase().includes(searchTerm.toLowerCase()) ||
    t.specialty.toLowerCase().includes(searchTerm.toLowerCase())
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
            placeholder="Buscar por nombre o especialidad..." 
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
          Añadir Profesor
        </button>
      </div>

      <div className="glass-card overflow-hidden">
        {loading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
            <p className="text-surface-500 font-medium">Cargando profesores...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-surface-50 border-b border-surface-200">
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Docente</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Especialidad</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Contratación</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-100">
                <AnimatePresence>
                  {filteredTeachers.map((teacher) => (
                    <motion.tr 
                      key={teacher.id}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      className="hover:bg-surface-50/50 transition-colors group"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-full bg-indigo-50 flex items-center justify-center text-indigo-600 font-bold overflow-hidden">
                            <img src={`https://ui-avatars.com/api/?name=${teacher.firstName}+${teacher.lastName}&background=6366f1&color=fff`} alt="" />
                          </div>
                          <div className="flex flex-col">
                            <span className="font-semibold text-surface-900">{teacher.firstName} {teacher.lastName}</span>
                            <span className="text-xs text-surface-400">ID: {teacher.id.split('-')[0]}</span>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700">
                          <BookOpen size={12} />
                          {teacher.specialty}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-sm text-surface-600">
                        <div className="flex items-center gap-1.5">
                          <Calendar size={14} className="text-surface-400" />
                          {new Date(teacher.hireDate).toLocaleDateString('es-ES', { year: 'numeric', month: 'long', day: 'numeric' })}
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button 
                            onClick={() => handleEdit(teacher)}
                            className="p-2 hover:bg-brand-50 text-brand-600 rounded-lg transition-colors"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            onClick={() => handleDelete(teacher.id)}
                            className="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
                {!loading && filteredTeachers.length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-6 py-12 text-center text-surface-400">
                      No se encontraron profesores.
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
                <h3 className="text-xl font-bold">{editing ? 'Editar Profesor' : 'Nuevo Profesor'}</h3>
                <button onClick={resetForm} className="p-2 hover:bg-surface-200 rounded-full transition-colors">
                  <X size={20} />
                </button>
              </div>
              <form onSubmit={handleSubmit} className="p-8 space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-surface-700">Nombre</label>
                    <input 
                      value={form.firstName} 
                      onChange={e => setForm({ ...form, firstName: e.target.value })} 
                      placeholder="Nombre" 
                      className="input-field w-full" 
                      required 
                    />
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-surface-700">Apellidos</label>
                    <input 
                      value={form.lastName} 
                      onChange={e => setForm({ ...form, lastName: e.target.value })} 
                      placeholder="Apellidos" 
                      className="input-field w-full" 
                      required 
                    />
                  </div>
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-surface-700">Especialidad</label>
                  <input 
                    value={form.specialty} 
                    onChange={e => setForm({ ...form, specialty: e.target.value })} 
                    placeholder="Ej. Matemáticas, Inglés..." 
                    className="input-field w-full" 
                  />
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-surface-700">Fecha de Contratación</label>
                  <input 
                    type="date"
                    value={form.hireDate} 
                    onChange={e => setForm({ ...form, hireDate: e.target.value })} 
                    className="input-field w-full" 
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <button type="button" onClick={resetForm} className="btn-secondary flex-1">Cancelar</button>
                  <button type="submit" className="btn-primary flex-1">
                    {editing ? 'Actualizar' : 'Registrar Profesor'}
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