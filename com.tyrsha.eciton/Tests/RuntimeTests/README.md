# Runtime Tests

## CLI(배치모드)에서 테스트 실행

Unity 에디터 CLI에서 테스트 실행 예시:

```bash
Unity -batchmode -nographics -quit \
  -projectPath /path/to/your/unity-project \
  -runTests -testPlatform PlayMode \
  -testResults "TestResults.xml" \
  -logFile "unity-tests.log"
```

패키지 단독 레포는 Unity 프로젝트가 아니므로, 실제로는 **이 패키지를 포함한 Unity 프로젝트**에서 위 커맨드로 실행하면 됩니다.
