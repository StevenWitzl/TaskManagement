import { useState, type DragEvent, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { apiFetch } from '../api'
import { useAuth } from '../AuthContext'
import { useRealtime } from '../useRealtime'
import { PRIORITY_LABELS, Priority, type TaskDto } from '../models'

const formatDate = (iso: string) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

export function Tasks() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const { tasks, connected, error: connectionError } = useRealtime(user?.token ?? null)

  const [error, setError] = useState<string | null>(null)
  const [showAddModal, setShowAddModal] = useState(false)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState<Priority>(Priority.Medium)
  const [completingTaskId, setCompletingTaskId] = useState<string | null>(null)
  const [completionDescription, setCompletionDescription] = useState('')
  const [draggingId, setDraggingId] = useState<string | null>(null)
  const [dragOverId, setDragOverId] = useState<string | null>(null)

  const openTasks = tasks.filter((t) => !t.completedDate)
  const completedTasks = tasks.filter((t) => t.completedDate)
  const countByPriority = (p: Priority) => openTasks.filter((t) => t.priority === p).length
  const token = user?.token

  const run = async (action: () => Promise<unknown>) => {
    setError(null)
    try {
      await action()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong. Please try again.')
    }
  }

  const openAddModal = () => {
    setTitle('')
    setDescription('')
    setPriority(Priority.Medium)
    setError(null)
    setShowAddModal(true)
  }

  const addTask = async (event: FormEvent) => {
    event.preventDefault()
    if (!title.trim() || !description.trim()) {
      setError('Title and description are required.')
      return
    }
    await run(async () => {
      await apiFetch('/api/tasks', { method: 'POST', body: { title, description, priority }, token })
      setShowAddModal(false)
    })
  }

  const confirmComplete = async (task: TaskDto) => {
    await run(async () => {
      await apiFetch(`/api/tasks/${task.id}/complete`, {
        method: 'POST',
        body: { completedDescription: completionDescription.trim() || null },
        token,
      })
      setCompletingTaskId(null)
    })
  }

  const remove = (task: TaskDto) => run(() => apiFetch(`/api/tasks/${task.id}`, { method: 'DELETE', token }))

  // Drag & drop reordering: dropping onto a card moves the dragged task to that card's position.
  const handleDragStart = (event: DragEvent, task: TaskDto) => {
    event.dataTransfer.effectAllowed = 'move'
    event.dataTransfer.setData('text/plain', task.id)
    setDraggingId(task.id)
  }

  const handleDragOver = (event: DragEvent, task: TaskDto) => {
    event.preventDefault()
    event.dataTransfer.dropEffect = 'move'
    if (task.id !== dragOverId) setDragOverId(task.id)
  }

  const handleDrop = async (event: DragEvent, target: TaskDto) => {
    event.preventDefault()
    const draggedId = event.dataTransfer.getData('text/plain') || draggingId
    setDraggingId(null)
    setDragOverId(null)
    if (!draggedId || draggedId === target.id) return
    await run(() =>
      apiFetch(`/api/tasks/${draggedId}/reorder`, { method: 'POST', body: { newOrder: target.order }, token }),
    )
  }

  const handleDragEnd = () => {
    setDraggingId(null)
    setDragOverId(null)
  }

  const signOut = () => {
    logout()
    navigate('/')
  }

  return (
    <>
      <header className="topbar">
        <div>
          <h1>My Tasks</h1>
          <p className="signed-in">
            {user?.firstName} {user?.lastName} · {user?.email}
          </p>
          <div className="stats">
            <span className="stat p-2">High {countByPriority(Priority.High)}</span>
            <span className="stat p-1">Medium {countByPriority(Priority.Medium)}</span>
            <span className="stat p-0">Low {countByPriority(Priority.Low)}</span>
            <span className="stat done">Completed {completedTasks.length}</span>
            <span className="stat open">Outstanding {openTasks.length}</span>
          </div>
        </div>
        <div className="topbar-right">
          <span className={connected ? 'status online' : 'status'}>{connected ? 'Live' : 'Offline'}</span>
          <button type="button" className="ghost" onClick={signOut}>
            Sign out
          </button>
        </div>
      </header>

      <main className="page">
        {(error ?? connectionError) && <p className="error">{error ?? connectionError}</p>}

        <div className="toolbar">
          <button type="button" className="primary" onClick={openAddModal}>
            + Create task
          </button>
        </div>

        <section>
          <h2>
            Open <span className="count">{openTasks.length}</span>
          </h2>
          {openTasks.length === 0 && <p className="empty">Nothing open — create a task to get started.</p>}
          {openTasks.map((task) => (
            <article
              className={`task${draggingId === task.id ? ' dragging' : ''}${dragOverId === task.id && draggingId !== task.id ? ' drag-over' : ''}`}
              key={task.id}
              draggable
              onDragStart={(e) => handleDragStart(e, task)}
              onDragOver={(e) => handleDragOver(e, task)}
              onDragLeave={() => setDragOverId((id) => (id === task.id ? null : id))}
              onDrop={(e) => handleDrop(e, task)}
              onDragEnd={handleDragEnd}
            >
              <div className="order-controls" title="Drag to reorder">
                <span className="grip" aria-hidden="true">⠿</span>
                <span className="order">{task.order}</span>
              </div>
              <div className="body">
                <div className="title-row">
                  <h3>{task.title}</h3>
                  <span className={`priority p-${task.priority}`}>{PRIORITY_LABELS[task.priority]}</span>
                </div>
                <p className="description">{task.description}</p>
                <p className="meta">Created {formatDate(task.createdDate)}</p>

                {completingTaskId === task.id ? (
                  <div className="complete-form">
                    <input
                      name="completionDescription"
                      value={completionDescription}
                      onChange={(e) => setCompletionDescription(e.target.value)}
                      placeholder="How was it completed? (optional)"
                    />
                    <button type="button" className="primary" onClick={() => confirmComplete(task)}>
                      Complete
                    </button>
                    <button type="button" className="ghost" onClick={() => setCompletingTaskId(null)}>
                      Cancel
                    </button>
                  </div>
                ) : (
                  <div className="actions">
                    <button
                      type="button"
                      className="primary"
                      onClick={() => {
                        setCompletingTaskId(task.id)
                        setCompletionDescription('')
                      }}
                    >
                      Mark complete
                    </button>
                    <button type="button" className="danger" onClick={() => remove(task)}>
                      Delete
                    </button>
                  </div>
                )}
              </div>
            </article>
          ))}
        </section>

        {completedTasks.length > 0 && (
          <section>
            <h2>
              Completed <span className="count">{completedTasks.length}</span>
            </h2>
            {completedTasks.map((task) => (
              <article className="task done" key={task.id}>
                <div className="order-controls">
                  <span className="order">{task.order}</span>
                </div>
                <div className="body">
                  <div className="title-row">
                    <h3>{task.title}</h3>
                    <span className={`priority p-${task.priority}`}>{PRIORITY_LABELS[task.priority]}</span>
                  </div>
                  <p className="description">{task.description}</p>
                  <p className="meta">
                    Completed {task.completedDate ? formatDate(task.completedDate) : ''}
                    {task.completedDescription ? ` — ${task.completedDescription}` : ''}
                  </p>
                  <div className="actions">
                    <button type="button" className="danger" onClick={() => remove(task)}>
                      Delete
                    </button>
                  </div>
                </div>
              </article>
            ))}
          </section>
        )}
      </main>

      {showAddModal && (
        <div className="modal-backdrop" onClick={() => setShowAddModal(false)}>
          <section className="modal" role="dialog" aria-label="Create task" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Create task</h2>
              <button type="button" className="close" aria-label="Close" onClick={() => setShowAddModal(false)}>
                ×
              </button>
            </div>
            <form onSubmit={addTask}>
              <div className="row">
                <label className="grow">
                  Title
                  <input
                    name="title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    placeholder="What needs doing?"
                    autoFocus
                    required
                  />
                </label>
                <label>
                  Priority
                  <select name="priority" value={priority} onChange={(e) => setPriority(Number(e.target.value) as Priority)}>
                    <option value={Priority.Low}>Low</option>
                    <option value={Priority.Medium}>Medium</option>
                    <option value={Priority.High}>High</option>
                  </select>
                </label>
              </div>
              <label>
                Description
                <textarea
                  name="description"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={3}
                  placeholder="Add some detail…"
                  required
                />
              </label>
              <div className="modal-actions">
                <button type="button" className="ghost" onClick={() => setShowAddModal(false)}>
                  Cancel
                </button>
                <button className="primary" type="submit">
                  Add task
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
    </>
  )
}
