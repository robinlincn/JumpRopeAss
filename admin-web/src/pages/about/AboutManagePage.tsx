import { Button, Card, Form, Input, Space, Typography, Upload, Select, message, Modal } from 'antd'
import type { UploadFile } from 'antd/es/upload/interface'
import {
  BoldOutlined,
  ItalicOutlined,
  UnderlineOutlined,
  StrikethroughOutlined,
  AlignLeftOutlined,
  AlignCenterOutlined,
  AlignRightOutlined,
  OrderedListOutlined,
  UnorderedListOutlined,
  LinkOutlined,
  CheckSquareOutlined,
  HighlightOutlined,
  EditOutlined,
  SmileOutlined,
  PictureOutlined,
  VideoCameraOutlined,
  TableOutlined,
  UndoOutlined,
  RedoOutlined,
  FullscreenOutlined,
  FullscreenExitOutlined,
  PlusOutlined,
} from '@ant-design/icons'
import { useCallback, useEffect, useRef, useState } from 'react'
import { apiFetch, apiUpload } from '../../lib/api'

type FormValues = {
  name: string
  address?: string
  logoUrl?: string
  overview?: string
  history?: string
  honors?: string
}

function RichEditor({ value, onChange }: { value?: string; onChange?: (v: string) => void }) {
  const editorRef = useRef<HTMLDivElement | null>(null)
  const [fullscreen, setFullscreen] = useState(false)
  const apply = (cmd: string, arg?: string) => {
    editorRef.current?.focus()
    document.execCommand(cmd, false, arg)
    const v = editorRef.current?.innerHTML ?? ''
    onChange?.(v)
  }
  const insertHTML = (html: string) => {
    editorRef.current?.focus()
    document.execCommand('insertHTML', false, html)
    const v = editorRef.current?.innerHTML ?? ''
    onChange?.(v)
  }
  return (
    <Card style={{ borderRadius: 10 }} styles={{ body: { padding: 10 } }}>
      <Space wrap style={{ marginBottom: 8 }}>
        <Select
          size="small"
          style={{ width: 100 }}
          placeholder="H"
          onChange={(v) => apply('formatBlock', v)}
          options={[
            { value: 'h1', label: 'H1' },
            { value: 'h2', label: 'H2' },
            { value: 'h3', label: 'H3' },
            { value: 'p', label: '正文' },
          ]}
        />
        <Button size="small" icon={<BoldOutlined />} onClick={() => apply('bold')} />
        <Button size="small" icon={<ItalicOutlined />} onClick={() => apply('italic')} />
        <Button size="small" icon={<UnderlineOutlined />} onClick={() => apply('underline')} />
        <Button size="small" icon={<StrikethroughOutlined />} onClick={() => apply('strikeThrough')} />
        <Button size="small" icon={<AlignLeftOutlined />} onClick={() => apply('justifyLeft')} />
        <Button size="small" icon={<AlignCenterOutlined />} onClick={() => apply('justifyCenter')} />
        <Button size="small" icon={<AlignRightOutlined />} onClick={() => apply('justifyRight')} />
        <Button size="small" icon={<EditOutlined />} onClick={() => apply('removeFormat')} />
        <Button size="small" icon={<HighlightOutlined />} onClick={() => apply('foreColor', '#ff6b00')} />
        <Button size="small" icon={<UnorderedListOutlined />} onClick={() => apply('insertUnorderedList')} />
        <Button size="small" icon={<OrderedListOutlined />} onClick={() => apply('insertOrderedList')} />
        <Button size="small" icon={<LinkOutlined />} onClick={() => {
          const url = window.prompt('输入链接地址') || ''
          if (url) apply('createLink', url)
        }} />
        <Button size="small" icon={<CheckSquareOutlined />} onClick={() => insertHTML('<span>☑</span> ')} />
        <Button size="small" icon={<SmileOutlined />} onClick={() => insertHTML('🙂')} />
        <Upload accept="image/*" maxCount={1} beforeUpload={() => false} showUploadList={false} onChange={(info) => {
          const f = info.file.originFileObj as File | undefined
          if (!f) return
          if (f.size > 2 * 1024 * 1024) {
            message.error('图片过大，请选择小于2MB的图片')
            return
          }
          const fd = new FormData()
          fd.append('file', f)
          apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd).then((resp) => {
            if (resp.code !== 0) {
              message.error(resp.message || '上传失败')
              return
            }
            insertHTML(`<img src="${resp.data.url}" style="max-width:100%;border-radius:8px;" />`)
          })
        }}>
          <Button size="small" icon={<PictureOutlined />} />
        </Upload>
        <Upload accept="video/*" maxCount={1} beforeUpload={() => false} showUploadList={false} onChange={(info) => {
          const f = info.file.originFileObj as File | undefined
          if (!f) return
          const url = URL.createObjectURL(f)
          insertHTML(`<video src="${url}" controls style="max-width:100%;border-radius:8px;"></video>`)
        }}>
          <Button size="small" icon={<VideoCameraOutlined />} />
        </Upload>
        <Button size="small" icon={<TableOutlined />} onClick={() => insertHTML('<table style="width:100%;border-collapse:collapse;"><tr><th style="border:1px solid #ddd;padding:6px;">列1</th><th style="border:1px solid #ddd;padding:6px;">列2</th></tr><tr><td style="border:1px solid #ddd;padding:6px;">内容</td><td style="border:1px solid #ddd;padding:6px;">内容</td></tr></table>')} />
        <Button size="small" icon={<UndoOutlined />} onClick={() => apply('undo')} />
        <Button size="small" icon={<RedoOutlined />} onClick={() => apply('redo')} />
        <Button size="small" icon={fullscreen ? <FullscreenExitOutlined /> : <FullscreenOutlined />} onClick={() => setFullscreen((v) => !v)} />
      </Space>
      <div
        ref={editorRef}
        contentEditable
        onInput={(e) => {
          const v = (e.target as HTMLDivElement).innerHTML
          onChange?.(v)
        }}
        style={{
          minHeight: 220,
          border: '1px solid #E5E7EB',
          borderRadius: 8,
          padding: 12,
          ...(fullscreen ? { position: 'fixed', inset: 20, zIndex: 1000, background: '#fff', overflow: 'auto' } : {}),
        }}
        dangerouslySetInnerHTML={{ __html: value || '' }}
      />
      {/* 去除占位提示 */}
    </Card>
  )
}

export function AboutManagePage() {
  const [form] = Form.useForm<FormValues & { logoFileList?: UploadFile[] }>()
  const [loading, setLoading] = useState(false)

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const res = await apiFetch<FormValues>('/api/v1/admin/about')
      if (res.code === 0 && res.data) {
        form.setFieldsValue(res.data)
        if (res.data.logoUrl) {
          const fileList: UploadFile[] = [{
              uid: '-1',
              name: 'logo.png',
              status: 'done',
              url: res.data.logoUrl,
          }]
          form.setFieldsValue({ logoFileList: fileList })
        }
      }
    } catch (e) {
      console.error(e)
      message.error('加载失败')
    } finally {
      setLoading(false)
    }
  }, [form])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }} loading={loading}>
        <Form form={form} layout="vertical" initialValues={{ name: '', address: '' }}>
          <Form.Item
            name="logoFileList"
            label="协会Logo"
            valuePropName="fileList"
            getValueFromEvent={(e) => e?.fileList}
            rules={[
              {
                validator: async () => {
                  const logoUrl = form.getFieldValue('logoUrl')
                  if (!logoUrl) throw new Error('请上传协会Logo')
                },
              },
            ]}
          >
            <Upload
              listType="picture-card"
              maxCount={1}
              accept="image/*"
              beforeUpload={() => false}
              onChange={(info) => {
                if (info.fileList.length === 0) {
                  form.setFieldsValue({ logoUrl: undefined })
                  return
                }
                const f = info.file.originFileObj as File | undefined
                if (!f) return
                if (f.size > 2 * 1024 * 1024) {
                  message.error('图片过大，请选择小于2MB的图片')
                  form.setFieldsValue({ logoFileList: [], logoUrl: undefined })
                  return
                }
                const fd = new FormData()
                fd.append('file', f)
                apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd).then((resp) => {
                  if (resp.code !== 0) {
                    message.error(resp.message || '上传失败')
                    form.setFieldsValue({ logoFileList: [], logoUrl: undefined })
                    return
                  }
                  form.setFieldsValue({ logoUrl: resp.data.url })
                })
              }}
            >
              <div>
                <PlusOutlined />
                <div style={{ marginTop: 8 }}>上传Logo</div>
              </div>
            </Upload>
          </Form.Item>
          <Form.Item name="logoUrl" hidden>
            <Input />
          </Form.Item>

          <Form.Item name="name" label="协会名称" rules={[{ required: true, message: '请输入协会名称' }]}>
            <Input placeholder="请输入协会名称" />
          </Form.Item>
          <Form.Item name="address" label="协会地址">
            <Input placeholder="请输入协会地址" />
          </Form.Item>

          <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>协会概述</Typography.Text>
              <Form.Item name="overview" valuePropName="value" rules={[{ required: true, message: '请输入协会概述' }]} noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>

          <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>协会历史</Typography.Text>
              <Form.Item name="history" valuePropName="value" rules={[{ required: true, message: '请输入协会历史' }]} noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>

          <Card style={{ borderRadius: 14 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>协会荣誉</Typography.Text>
              <Form.Item name="honors" valuePropName="value" rules={[{ required: true, message: '请输入协会荣誉' }]} noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>
          <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
            <Button
              type="primary"
              icon={<EditOutlined />}
              onClick={async () => {
                Modal.confirm({
                  title: '您确定要保存吗？',
                  okText: '保存',
                  cancelText: '取消',
                  onOk: async () => {
                    let v: FormValues & { logoFileList?: UploadFile[] }
                    try {
                      v = await form.validateFields()
                    } catch (e) {
                      message.error('请完善必填项后再保存')
                      throw e
                    }
                    const payload: FormValues = {
                      name: v.name,
                      address: v.address,
                      logoUrl: v.logoUrl,
                      overview: v.overview,
                      history: v.history,
                      honors: v.honors,
                    }
                    try {
                      const res = await apiFetch('/api/v1/admin/about', {
                        method: 'POST',
                        body: JSON.stringify(payload),
                      })
                      if (res.code === 0) {
                        message.success('已保存')
                      } else {
                        message.error(res.message || '保存失败')
                      }
                    } catch (e) {
                      console.error(e)
                      message.error('保存失败')
                    }
                  },
                })
              }}
            >
              保存
            </Button>
          </Space>
        </Form>
      </Card>
    </Space>
  )
}
