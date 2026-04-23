import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Plus, 
  Search, 
  Edit2, 
  Trash2, 
  X,
  Layout
} from 'lucide-react'
import { studentsApi } from '../services/api'
import type { Student } from '../types'

export default function Students() {
  const [students, setStudents] = useState<Student[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<Student | null>(null)
  const [form, setForm] = useState({ firstName: '', lastName: '', dateOfBirth: '', classroomId: '' })
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    loadStudents()
  }, [])

  const loadStudents = async () => {
    try {
      const res = await studentsApi.getAll()
      setStudents(res.data)
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
        await studentsApi.update(editing.id, form)
      } else {
        await studentsApi.create(form)
      }
      resetForm()
      loadStudents()
    } catch (e) {
      console.error(e)
    }
  }

  const resetForm = () => {
    setForm({ firstName: '', lastName: '', dateOfBirth: '', classroomId: '' })
    setShowForm(false)
    setEditing(null)
  }

  const handleDelete = async (id: string) => {
    if (confirm('¿Deseas eliminar permanentemente la ficha de este alumno?')) {
      try {
        await studentsApi.delete(id)
        loadStudents()
      } catch (e) {
        console.error(e)
      }
    }
  }

  const handleEdit = (student: Student) => {
    setEditing(student)
    setForm({ 
      firstName: student.firstName, 
      lastName: student.lastName, 
      dateOfBirth: student.dateOfBirth, 
      classroomId: student.classroomId || '' 
    })
    setShowForm(true)
  }

  const filteredStudents = students.filter(s => 
    (s.firstName + ' ' + s.lastName).toLowerCase().includes(searchTerm.toLowerCase())
  )

  const calculateAge = (dob: string) => {
    if (!dob) return 'N/A'
    const birthDate = new Date(dob)
    const today = new Date()
    let age = today.getFullYear() - birthDate.getFullYear()
    const m = today.getMonth() - birthDate.getMonth()
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) age--
    return age + ' años'
  }

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
            placeholder="Buscar por nombre o apellido..." 
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
          Nuevo Alumno
        </button>
      </div>

      <div className="glass-card overflow-hidden">
        {loading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
            <p className="text-surface-500 font-medium">Cargando base de alumnos...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-surface-50 border-b border-surface-200">
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Estudiante</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Edad</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Aula Asignada</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-100">
                <AnimatePresence>
                  {filteredStudents.map((student) => (
                    <motion.tr 
                      key={student.id}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      className="hover:bg-surface-50/50 transition-colors group"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-600 font-bold overflow-hidden">
                            <img src={`https://ui-avatars.com/api/?name=${student.firstName}+${student.lastName}&background=3b82f6&color=fff`} alt="" />
                          </div>
                          <div className="flex flex-col">
                            <span className="font-semibold text-surface-900">{student.firstName} {student.lastName}</span>
                            <span className="text-xs text-surface-400">Nacido: {student.dateOfBirth}</span>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-sm text-surface-600">
                        {calculateAge(student.dateOfBirth)}
                      </td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium ${student.classroomId ? 'bg-emerald-50 text-emerald-700' : 'bg-surface-100 text-surface-500'}`}>
                          <Layout size={12} />
                          {student.classroomId ? 'Asignado' : 'Pendiente'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button 
                            onClick={() => handleEdit(student)}
                            className="p-2 hover:bg-brand-50 text-brand-600 rounded-lg transition-colors"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            onClick={() => handleDelete(student.id)}
                            className="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
                {!loading && filteredStudents.length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-6 py-12 text-center text-surface-400">
                      No se encontraron alumnos en la base de datos.
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
                <h3 className="text-xl font-bold">{editing ? 'Editar Ficha Alumno' : 'Nueva Matriculación'}</h3>
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
                  <label className="text-sm font-semibold text-surface-700">Fecha de Nacimiento</label>
                  <input 
                    type="date"
                    value={form.dateOfBirth} 
                    onChange={e => setForm({ ...form, dateOfBirth: e.target.value })} 
                    className="input-field w-full" 
                  />
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-surface-700">ID de Aula (Opcional)</label>
                  <input 
                    value={form.classroomId} 
                    onChange={e => setForm({ ...form, classroomId: e.target.value })} 
                    placeholder="ID Aula" 
                    className="input-field w-full" 
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <button type="button" onClick={resetForm} className="btn-secondary flex-1">Cancelar</button>
                  <button type="submit" className="btn-primary flex-1">
                    {editing ? 'Guardar Cambios' : 'Completar Matrícula'}
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