import axios from 'axios'

const API_URL = 'http://localhost:8080/api'

const api = axios.create({
  baseURL: API_URL,
})

export const schoolsApi = {
  getAll: () => api.get('/schools'),
  getById: (id: string) => api.get(`/schools/${id}`),
  create: (data: any) => api.post('/schools', data),
  update: (id: string, data: any) => api.put(`/schools/${id}`, data),
  delete: (id: string) => api.delete(`/schools/${id}`),
}

export const teachersApi = {
  getAll: () => api.get('/teachers'),
  getById: (id: string) => api.get(`/teachers/${id}`),
  create: (data: any) => api.post('/teachers', data),
  update: (id: string, data: any) => api.put(`/teachers/${id}`, data),
  delete: (id: string) => api.delete(`/teachers/${id}`),
  updateAvailability: (id: string, data: any[]) => api.put(`/teachers/${id}/availability`, data),
}

export const studentsApi = {
  getAll: () => api.get('/students'),
  getById: (id: string) => api.get(`/students/${id}`),
  create: (data: any) => api.post('/students', data),
  update: (id: string, data: any) => api.put(`/students/${id}`, data),
  delete: (id: string) => api.delete(`/students/${id}`),
}

export const classroomsApi = {
  getAll: () => api.get('/classrooms'),
  getById: (id: string) => api.get(`/classrooms/${id}`),
  create: (data: any) => api.post('/classrooms', data),
  update: (id: string, data: any) => api.put(`/classrooms/${id}`, data),
  delete: (id: string) => api.delete(`/classrooms/${id}`),
}

export const roomsApi = {
  getAll: () => api.get('/rooms'),
  getById: (id: string) => api.get(`/rooms/${id}`),
  create: (data: any) => api.post('/rooms', data),
  update: (id: string, data: any) => api.put(`/rooms/${id}`, data),
  delete: (id: string) => api.delete(`/rooms/${id}`),
}

export const invoicesApi = {
  getAll: () => api.get('/invoices'),
  getById: (id: string) => api.get(`/invoices/${id}`),
  create: (data: any) => api.post('/invoices', data),
  update: (id: string, data: any) => api.put(`/invoices/${id}`, data),
  delete: (id: string) => api.delete(`/invoices/${id}`),
}
export const maintenanceApi = {
  getStats: () => api.get('/maintenance/stats'),
  reset: () => api.post('/maintenance/reset'),
  clear: () => api.delete('/maintenance/clear'),
}

export const curriculumApi = {
  getAll: () => api.get('/curriculum'),
  getByGrade: (grade: string) => api.get(`/curriculum/${grade}`),
  getSubjects: () => api.get('/subjects'),
}

export const schedulesApi = {
  getAll: () => api.get('/schedules'),
  getByClassroom: (classroomId: string) => api.get(`/schedules/classroom/${classroomId}`),
  getByTeacher: (teacherId: string) => api.get(`/schedules/teacher/${teacherId}`),
  getByRoom: (roomId: string) => api.get(`/schedules/room/${roomId}`),
  generate: (classroomId: string, sessionType: number) => api.post(`/schedules/generate?classroomId=${classroomId}&sessionType=${sessionType}`),
  generateAll: (sessionType: number) => api.post(`/schedules/generate-all?sessionType=${sessionType}`),
  validate: (sessionType: number) => api.post(`/schedules/validate?sessionType=${sessionType}`),
  getScore: (sessionType: number) => api.get(`/schedules/score?sessionType=${sessionType}`),
  update: (id: string, data: any) => api.put(`/schedules/${id}`, data),
}

export const timeSlotsApi = {
  getAll: () => api.get('/timeslots'),
}

export const classUnitsApi = {
  getAll: () => api.get('/classunits'),
  getById: (id: string) => api.get(`/classunits/${id}`),
  getByClassroom: (classroomId: string) => api.get(`/classunits/classroom/${classroomId}`),
  create: (data: any) => api.post('/classunits', data),
  generateFromCurriculum: () => api.post('/classunits/generate-from-curriculum'),
  update: (id: string, data: any) => api.put(`/classunits/${id}`, data),
  delete: (id: string) => api.delete(`/classunits/${id}`),
}

export const timetableFrameworksApi = {
  getAll: () => api.get('/frameworks'),
  getById: (id: string) => api.get(`/frameworks/${id}`),
  create: (data: any) => api.post('/frameworks', data),
  update: (id: string, data: any) => api.put(`/frameworks/${id}`, data),
  delete: (id: string) => api.delete(`/frameworks/${id}`),
  generateSlots: (id: string) => api.post(`/frameworks/${id}/generate-slots`),
}
