import { Button, Card, Form, Input, Select, Space, Switch, Tabs, Typography, message } from 'antd'
import { useEffect, useState } from 'react'
import { apiFetch } from '../../lib/api'

type SystemSettings = {
  siteName?: string | null
  siteShortName?: string | null
  supportPhone?: string | null
  logoUrl?: string | null
  defaultEntryNoticeHtml?: string | null
  certQueryPrefix?: string | null
  sms?: {
    provider?: string | null
    accessKeyId?: string | null
    accessKeySecret?: string | null
    signName?: string | null
    templateCode?: string | null
  } | null
  storage?: {
    provider?: string | null
    publicBaseUrl?: string | null
    localBasePath?: string | null
    ossEndpoint?: string | null
    ossBucket?: string | null
    ossAccessKeyId?: string | null
    ossAccessKeySecret?: string | null
  } | null
}

type PaymentSettings = {
  enabled: boolean
  provider?: string | null
  wechatMchId?: string | null
  wechatAppId?: string | null
  wechatSerialNo?: string | null
  wechatNotifyUrl?: string | null
  wechatApiV3Key?: string | null
  wechatPrivateKeyPem?: string | null
}

type SettingsResp = {
  system: SystemSettings
  payment: PaymentSettings
}

type FormValues = {
  siteName?: string
  siteShortName?: string
  supportPhone?: string
  logoUrl?: string
  defaultEntryNoticeHtml?: string
  certQueryPrefix?: string
  smsProvider?: string
  smsAccessKeyId?: string
  smsAccessKeySecret?: string
  smsSignName?: string
  smsTemplateCode?: string
  storageProvider?: string
  storagePublicBaseUrl?: string
  storageLocalBasePath?: string
  storageOssEndpoint?: string
  storageOssBucket?: string
  storageOssAccessKeyId?: string
  storageOssAccessKeySecret?: string
  payEnabled?: boolean
  payProvider?: string
  wechatMchId?: string
  wechatAppId?: string
  wechatSerialNo?: string
  wechatNotifyUrl?: string
  wechatApiV3Key?: string
  wechatPrivateKeyPem?: string
}

export function SystemSettingsPage() {
  const [form] = Form.useForm<FormValues>()
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [activeTab, setActiveTab] = useState('system')

  const load = async () => {
    setLoading(true)
    try {
      const res = await apiFetch<SettingsResp>('/api/v1/admin/system-settings')
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      const d = res.data
      form.setFieldsValue({
        siteName: d?.system?.siteName ?? '',
        siteShortName: d?.system?.siteShortName ?? '',
        supportPhone: d?.system?.supportPhone ?? '',
        logoUrl: d?.system?.logoUrl ?? '',
        defaultEntryNoticeHtml: d?.system?.defaultEntryNoticeHtml ?? '',
        certQueryPrefix: d?.system?.certQueryPrefix ?? '',
        smsProvider: d?.system?.sms?.provider ?? 'none',
        smsAccessKeyId: d?.system?.sms?.accessKeyId ?? '',
        smsAccessKeySecret: d?.system?.sms?.accessKeySecret ?? '',
        smsSignName: d?.system?.sms?.signName ?? '',
        smsTemplateCode: d?.system?.sms?.templateCode ?? '',
        storageProvider: d?.system?.storage?.provider ?? 'local',
        storagePublicBaseUrl: d?.system?.storage?.publicBaseUrl ?? '',
        storageLocalBasePath: d?.system?.storage?.localBasePath ?? '',
        storageOssEndpoint: d?.system?.storage?.ossEndpoint ?? '',
        storageOssBucket: d?.system?.storage?.ossBucket ?? '',
        storageOssAccessKeyId: d?.system?.storage?.ossAccessKeyId ?? '',
        storageOssAccessKeySecret: d?.system?.storage?.ossAccessKeySecret ?? '',
        payEnabled: !!d?.payment?.enabled,
        payProvider: d?.payment?.provider ?? 'wechat',
        wechatMchId: d?.payment?.wechatMchId ?? '',
        wechatAppId: d?.payment?.wechatAppId ?? '',
        wechatSerialNo: d?.payment?.wechatSerialNo ?? '',
        wechatNotifyUrl: d?.payment?.wechatNotifyUrl ?? '',
        wechatApiV3Key: d?.payment?.wechatApiV3Key ?? '',
        wechatPrivateKeyPem: d?.payment?.wechatPrivateKeyPem ?? '',
      })
    } finally {
      setLoading(false)
    }
  }

  const save = async () => {
    const v = await form.validateFields()
    setSaving(true)
    try {
      const res = await apiFetch<any>('/api/v1/admin/system-settings', {
        method: 'PUT',
        body: JSON.stringify({
          system: {
            siteName: (v.siteName ?? '').trim() || null,
            siteShortName: (v.siteShortName ?? '').trim() || null,
            supportPhone: (v.supportPhone ?? '').trim() || null,
            logoUrl: (v.logoUrl ?? '').trim() || null,
            defaultEntryNoticeHtml: (v.defaultEntryNoticeHtml ?? '').trim() || null,
            certQueryPrefix: (v.certQueryPrefix ?? '').trim() || null,
            sms: {
              provider: (v.smsProvider ?? '').trim() || 'none',
              accessKeyId: (v.smsAccessKeyId ?? '').trim() || null,
              accessKeySecret: (v.smsAccessKeySecret ?? '').trim() || null,
              signName: (v.smsSignName ?? '').trim() || null,
              templateCode: (v.smsTemplateCode ?? '').trim() || null,
            },
            storage: {
              provider: (v.storageProvider ?? '').trim() || 'local',
              publicBaseUrl: (v.storagePublicBaseUrl ?? '').trim() || null,
              localBasePath: (v.storageLocalBasePath ?? '').trim() || null,
              ossEndpoint: (v.storageOssEndpoint ?? '').trim() || null,
              ossBucket: (v.storageOssBucket ?? '').trim() || null,
              ossAccessKeyId: (v.storageOssAccessKeyId ?? '').trim() || null,
              ossAccessKeySecret: (v.storageOssAccessKeySecret ?? '').trim() || null,
            },
          },
          payment: {
            enabled: !!v.payEnabled,
            provider: (v.payProvider ?? '').trim() || 'wechat',
            wechatMchId: (v.wechatMchId ?? '').trim() || null,
            wechatAppId: (v.wechatAppId ?? '').trim() || null,
            wechatSerialNo: (v.wechatSerialNo ?? '').trim() || null,
            wechatNotifyUrl: (v.wechatNotifyUrl ?? '').trim() || null,
            wechatApiV3Key: (v.wechatApiV3Key ?? '').trim() || null,
            wechatPrivateKeyPem: (v.wechatPrivateKeyPem ?? '').trim() || null,
          },
        }),
      })
      if (res.code !== 0) {
        message.error(res.message || '保存失败')
        return
      }
      message.success('已保存')
      await load()
    } finally {
      setSaving(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }}>
        <Space style={{ width: '100%', justifyContent: 'space-between' }} wrap>
          <div>
            <Typography.Title level={5} style={{ margin: 0 }}>
              系统设置
            </Typography.Title>
            <Typography.Text type="secondary">系统级参数与支付参数统一在此维护</Typography.Text>
          </div>
          <Space wrap>
            <Button onClick={load} loading={loading}>
              刷新
            </Button>
            <Button type="primary" onClick={save} loading={saving}>
              保存
            </Button>
          </Space>
        </Space>
      </Card>

      <Form form={form} layout="vertical">
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            {
              key: 'system',
              label: '系统',
              children: (
                <Card style={{ borderRadius: 16 }}>
                  <Form.Item name="siteName" label="系统名称">
                    <Input placeholder="例如：湖南省学生跳绳协会" />
                  </Form.Item>
                  <Form.Item name="siteShortName" label="简称">
                    <Input placeholder="例如：湖南跳绳" />
                  </Form.Item>
                  <Form.Item name="supportPhone" label="客服电话/咨询电话">
                    <Input placeholder="例如：400xxxxxxx" />
                  </Form.Item>
                  <Form.Item name="logoUrl" label="Logo URL">
                    <Input placeholder="用于前端展示的Logo地址（可为空）" />
                  </Form.Item>
                </Card>
              ),
            },
            {
              key: 'entry',
              label: '报名',
              children: (
                <Card style={{ borderRadius: 16 }}>
                  <Form.Item name="defaultEntryNoticeHtml" label="默认报名须知（支持HTML）">
                    <Input.TextArea rows={10} placeholder="例如：报名须知...\n支持HTML片段" />
                  </Form.Item>
                </Card>
              ),
            },
            {
              key: 'cert',
              label: '证书',
              children: (
                <Card style={{ borderRadius: 16 }}>
                  <Form.Item name="certQueryPrefix" label="证书查询链接前缀">
                    <Input placeholder="例如：https://xxx.com/#/cert/verify?certNo=" />
                  </Form.Item>
                </Card>
              ),
            },
            {
              key: 'payment',
              label: '支付',
              children: (
                <Card style={{ borderRadius: 16 }}>
                  <Form.Item name="payEnabled" label="启用支付" valuePropName="checked">
                    <Switch />
                  </Form.Item>
                  <Form.Item name="payProvider" label="支付方式">
                    <Select options={[{ value: 'wechat', label: '微信支付' }]} />
                  </Form.Item>
                  <Form.Item name="wechatMchId" label="微信商户号（MchId）">
                    <Input placeholder="例如：160xxxxxxx" />
                  </Form.Item>
                  <Form.Item name="wechatAppId" label="微信AppId（可选）">
                    <Input placeholder="例如：wx1234567890abcdef" />
                  </Form.Item>
                  <Form.Item name="wechatSerialNo" label="证书序列号（SerialNo）">
                    <Input placeholder="例如：777A..." />
                  </Form.Item>
                  <Form.Item name="wechatNotifyUrl" label="回调地址（NotifyUrl）">
                    <Input placeholder="例如：https://xxx.com/api/v1/pay/wechat/notify" />
                  </Form.Item>
                  <Form.Item name="wechatApiV3Key" label="APIv3Key（留空不修改）">
                    <Input.Password placeholder="留空不修改；显示为脱敏占位" />
                  </Form.Item>
                  <Form.Item name="wechatPrivateKeyPem" label="商户私钥（PEM，留空不修改）">
                    <Input.TextArea rows={6} placeholder="-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----" />
                  </Form.Item>
                </Card>
              ),
            },
            {
              key: 'sms',
              label: '短信',
              children: (
                <Card style={{ borderRadius: 16 }}>
                  <Form.Item name="smsProvider" label="短信服务商">
                    <Select
                      options={[
                        { value: 'none', label: '不启用' },
                        { value: 'aliyun', label: '阿里云短信' },
                        { value: 'tencent', label: '腾讯云短信' },
                      ]}
                    />
                  </Form.Item>
                  <Form.Item name="smsAccessKeyId" label="AccessKeyId">
                    <Input placeholder="留空不修改" />
                  </Form.Item>
                  <Form.Item name="smsAccessKeySecret" label="AccessKeySecret（留空不修改）">
                    <Input.Password placeholder="留空不修改；显示为脱敏占位" />
                  </Form.Item>
                  <Form.Item name="smsSignName" label="短信签名">
                    <Input placeholder="例如：湖南跳绳" />
                  </Form.Item>
                  <Form.Item name="smsTemplateCode" label="模板Code">
                    <Input placeholder="例如：SMS_123456789" />
                  </Form.Item>
                </Card>
              ),
            },
            {
              key: 'storage',
              label: '存储',
              children: (
                <Card style={{ borderRadius: 16 }}>
                  <Form.Item name="storageProvider" label="存储方式">
                    <Select
                      options={[
                        { value: 'local', label: '本地存储（/upload）' },
                        { value: 'oss', label: '阿里云 OSS' },
                      ]}
                    />
                  </Form.Item>
                  <Form.Item name="storagePublicBaseUrl" label="对外访问BaseUrl">
                    <Input placeholder="例如：https://static.xxx.com/upload" />
                  </Form.Item>
                  <Form.Item name="storageLocalBasePath" label="本地存储目录（可选）">
                    <Input placeholder="留空默认使用后端 upload 目录" />
                  </Form.Item>
                  <Form.Item name="storageOssEndpoint" label="OSS Endpoint">
                    <Input placeholder="例如：https://oss-cn-xxx.aliyuncs.com" />
                  </Form.Item>
                  <Form.Item name="storageOssBucket" label="OSS Bucket">
                    <Input placeholder="例如：my-bucket" />
                  </Form.Item>
                  <Form.Item name="storageOssAccessKeyId" label="OSS AccessKeyId">
                    <Input placeholder="留空不修改" />
                  </Form.Item>
                  <Form.Item name="storageOssAccessKeySecret" label="OSS AccessKeySecret（留空不修改）">
                    <Input.Password placeholder="留空不修改；显示为脱敏占位" />
                  </Form.Item>
                </Card>
              ),
            },
          ]}
        />
      </Form>
    </Space>
  )
}
