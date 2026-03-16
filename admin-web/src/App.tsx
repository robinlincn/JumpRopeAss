import { App as AntApp, ConfigProvider, theme } from 'antd'
import zhCN from 'antd/locale/zh_CN'
import { AppRouter } from './app/router/AppRouter'

function App() {
  return (
    <ConfigProvider
      locale={zhCN}
      theme={{
        algorithm: theme.defaultAlgorithm,
        token: {
          colorPrimary: '#1F6EC9',
          colorInfo: '#1F6EC9',
          colorSuccess: '#15854D',
          colorWarning: '#F59E0B',
          colorError: '#E53935',
          borderRadius: 10,
        },
        components: {
          Layout: {
            headerBg: '#ffffff',
            siderBg: '#ffffff',
          },
        },
      }}
    >
      <AntApp>
        <AppRouter />
      </AntApp>
    </ConfigProvider>
  )
}

export default App
